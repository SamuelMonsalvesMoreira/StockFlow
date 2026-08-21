import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, HostListener, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize, forkJoin } from 'rxjs';
import {
  Category,
  CreateCategoryRequest,
  CreateProductRequest,
  CreateSupplierRequest,
  CreateStockMovementRequest,
  InventorySummary,
  LoginRequest,
  Product,
  ReportOverview,
  StockMovement,
  StockMovementType,
  Supplier,
  UpdateProductRequest,
  UserRole,
  UserSession,
} from './models/inventory.models';
import { InventoryApiService } from './services/inventory-api.service';

type Toast = { type: 'success' | 'error'; message: string };

@Component({
  imports: [ReactiveFormsModule],
  selector: 'app-root',
  styleUrl: './app.css',
  templateUrl: './app.html',
})
export class App implements OnInit {
  private readonly api = inject(InventoryApiService);
  private readonly formBuilder = inject(FormBuilder);
  private toastTimer?: number;

  protected readonly products = signal<Product[]>([]);
  protected readonly currentUser = signal<UserSession | null>(null);
  protected readonly canManage = computed(() => this.currentUser()?.canManage === true);
  protected readonly summary = signal<InventorySummary>({
    totalProducts: 0,
    lowStockProducts: 0,
    totalUnits: 0,
    totalStockValue: 0,
  });
  protected readonly movements = signal<StockMovement[]>([]);
  protected readonly report = signal<ReportOverview | null>(null);
  protected readonly categories = signal<Category[]>([]);
  protected readonly suppliers = signal<Supplier[]>([]);
  protected readonly selectedProduct = signal<Product | null>(null);
  protected readonly editingProduct = signal<Product | null>(null);
  protected readonly lowStockOnly = signal(false);
  protected readonly isLoading = signal(true);
  protected readonly authChecked = signal(false);
  protected readonly isLoggingIn = signal(false);
  protected readonly authError = signal<string | null>(null);
  protected readonly isSubmitting = signal(false);
  protected readonly isLoadingMovements = signal(false);
  protected readonly isLoadingReport = signal(false);
  protected readonly isExportingReport = signal(false);
  protected readonly showProductForm = signal(false);
  protected readonly showMovementForm = signal(false);
  protected readonly showHistory = signal(false);
  protected readonly showReports = signal(false);
  protected readonly showCatalog = signal(false);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly toast = signal<Toast | null>(null);

  protected readonly searchControl = this.formBuilder.nonNullable.control('');

  protected readonly loginForm = this.formBuilder.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]],
  });

  protected readonly productForm = this.formBuilder.nonNullable.group({
    sku: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(40)]],
    name: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(120)]],
    unitPrice: [null as number | null, [Validators.required, Validators.min(0.01)]],
    minimumStock: [null as number | null, [Validators.required, Validators.min(0)]],
    maximumStock: [null as number | null, [Validators.required, Validators.min(0)]],
    categoryId: this.formBuilder.control<number | null>(null),
    supplierId: this.formBuilder.control<number | null>(null),
  });

  protected readonly categoryForm = this.formBuilder.nonNullable.group({
    name: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(80)]],
  });

  protected readonly supplierForm = this.formBuilder.nonNullable.group({
    name: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(120)]],
    contactName: ['', Validators.maxLength(120)],
    email: ['', [Validators.email, Validators.maxLength(160)]],
    phone: ['', Validators.maxLength(30)],
  });

  protected readonly movementForm = this.formBuilder.group({
    type: this.formBuilder.nonNullable.control<StockMovementType>('Entry'),
    quantity: this.formBuilder.nonNullable.control(1, [Validators.required, Validators.min(1)]),
    note: this.formBuilder.nonNullable.control('', Validators.maxLength(250)),
  });

  ngOnInit(): void {
    this.restoreSession();
  }

  protected login(): void {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    const value = this.loginForm.getRawValue();
    const request: LoginRequest = {
      email: value.email.trim(),
      password: value.password,
    };

    this.authError.set(null);
    this.isLoggingIn.set(true);
    this.api
      .login(request)
      .pipe(finalize(() => this.isLoggingIn.set(false)))
      .subscribe({
        next: (user) => {
          this.currentUser.set(user);
          this.loginForm.reset({ email: '', password: '' });
          this.refreshAll();
        },
        error: (error: HttpErrorResponse) => {
          this.authError.set(this.readError(error, 'E-mail ou senha incorretos.'));
        },
      });
  }

  protected loginAs(role: UserRole): void {
    const isManager = role === 'Manager';
    this.loginForm.setValue({
      email: isManager ? 'gestor@stockflow.local' : 'visitante@stockflow.local',
      password: isManager ? 'Gestor123!' : 'Visitante123!',
    });
    this.login();
  }

  protected logout(): void {
    this.isSubmitting.set(true);
    this.api
      .logout()
      .pipe(finalize(() => this.isSubmitting.set(false)))
      .subscribe({
        next: () => this.clearSession(),
        error: () => this.clearSession(),
      });
  }

  protected refreshAll(): void {
    if (!this.currentUser()) {
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set(null);

    forkJoin({
      products: this.api.getProducts(this.searchControl.value, this.lowStockOnly()),
      summary: this.api.getSummary(),
      categories: this.api.getCategories(),
      suppliers: this.api.getSuppliers(),
    })
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: ({ products, summary, categories, suppliers }) => {
          this.products.set(products);
          this.summary.set(summary);
          this.categories.set(categories);
          this.suppliers.set(suppliers);
        },
        error: (error: HttpErrorResponse) => {
          if (error.status === 401) {
            this.clearSession('Sua sessão expirou. Entre novamente.');
            return;
          }
          this.errorMessage.set(
            this.readError(error, 'Não foi possível conectar à API do StockFlow.'),
          );
        },
      });
  }

  protected applySearch(): void {
    this.refreshAll();
  }

  protected clearSearch(): void {
    this.searchControl.setValue('');
    this.refreshAll();
  }

  protected toggleLowStock(): void {
    this.lowStockOnly.update((value) => !value);
    this.refreshAll();
  }

  protected openCreateProduct(): void {
    if (!this.canManage()) {
      return;
    }
    this.editingProduct.set(null);
    this.productForm.reset({
      sku: '',
      name: '',
      unitPrice: null,
      minimumStock: null,
      maximumStock: null,
      categoryId: null,
      supplierId: null,
    });
    this.showProductForm.set(true);
  }

  protected openEditProduct(product: Product): void {
    if (!this.canManage()) {
      return;
    }
    this.editingProduct.set(product);
    this.productForm.reset({
      sku: product.sku,
      name: product.name,
      unitPrice: product.unitPrice,
      minimumStock: product.minimumStock,
      maximumStock: product.maximumStock,
      categoryId: product.categoryId,
      supplierId: product.supplierId,
    });
    this.showProductForm.set(true);
  }

  protected closeProductForm(): void {
    if (!this.isSubmitting()) {
      this.showProductForm.set(false);
    }
  }

  protected saveProduct(): void {
    if (!this.canManage()) {
      return;
    }
    if (this.productForm.invalid) {
      this.productForm.markAllAsTouched();
      return;
    }

    const value = this.productForm.getRawValue();
    if (Number(value.maximumStock) < Number(value.minimumStock)) {
      this.notify('error', 'O estoque máximo deve ser maior ou igual ao estoque mínimo.');
      return;
    }

    const request: UpdateProductRequest = {
      name: value.name.trim(),
      unitPrice: Number(value.unitPrice),
      minimumStock: Number(value.minimumStock),
      maximumStock: Number(value.maximumStock),
      categoryId: value.categoryId,
      supplierId: value.supplierId,
    };

    const productBeingEdited = this.editingProduct();
    const operation = productBeingEdited
      ? this.api.updateProduct(productBeingEdited.id, request)
      : this.api.createProduct({
          ...request,
          sku: value.sku.trim(),
        } satisfies CreateProductRequest);

    this.isSubmitting.set(true);
    operation.pipe(finalize(() => this.isSubmitting.set(false))).subscribe({
      next: (product) => {
        this.showProductForm.set(false);
        this.editingProduct.set(null);
        this.notify(
          'success',
          productBeingEdited
            ? `${product.name} foi atualizado com sucesso.`
            : `${product.name} foi cadastrado com sucesso.`,
        );
        this.refreshAll();
      },
      error: (error: HttpErrorResponse) => {
        this.notify('error', this.readError(error));
      },
    });
  }

  protected openCatalog(): void {
    if (!this.canManage()) {
      return;
    }
    this.categoryForm.reset({ name: '' });
    this.supplierForm.reset({ name: '', contactName: '', email: '', phone: '' });
    this.showCatalog.set(true);
  }

  protected closeCatalog(): void {
    if (!this.isSubmitting()) {
      this.showCatalog.set(false);
    }
  }

  protected createCategory(): void {
    if (!this.canManage()) {
      return;
    }
    if (this.categoryForm.invalid) {
      this.categoryForm.markAllAsTouched();
      return;
    }

    const request: CreateCategoryRequest = {
      name: this.categoryForm.controls.name.value.trim(),
    };
    this.isSubmitting.set(true);
    this.api
      .createCategory(request)
      .pipe(finalize(() => this.isSubmitting.set(false)))
      .subscribe({
        next: (category) => {
          this.categories.update((items) =>
            [...items, category].sort((a, b) => a.name.localeCompare(b.name)),
          );
          this.categoryForm.reset({ name: '' });
          this.notify('success', `Categoria ${category.name} adicionada.`);
        },
        error: (error: HttpErrorResponse) => this.notify('error', this.readError(error)),
      });
  }

  protected createSupplier(): void {
    if (!this.canManage()) {
      return;
    }
    if (this.supplierForm.invalid) {
      this.supplierForm.markAllAsTouched();
      return;
    }

    const value = this.supplierForm.getRawValue();
    const request: CreateSupplierRequest = {
      name: value.name.trim(),
      contactName: value.contactName.trim() || null,
      email: value.email.trim() || null,
      phone: value.phone.trim() || null,
    };
    this.isSubmitting.set(true);
    this.api
      .createSupplier(request)
      .pipe(finalize(() => this.isSubmitting.set(false)))
      .subscribe({
        next: (supplier) => {
          this.suppliers.update((items) =>
            [...items, supplier].sort((a, b) => a.name.localeCompare(b.name)),
          );
          this.supplierForm.reset({ name: '', contactName: '', email: '', phone: '' });
          this.notify('success', `Fornecedor ${supplier.name} adicionado.`);
        },
        error: (error: HttpErrorResponse) => this.notify('error', this.readError(error)),
      });
  }

  protected openMovement(product: Product, type: StockMovementType): void {
    if (!this.canManage()) {
      return;
    }
    this.selectedProduct.set(product);
    this.movementForm.reset({ type, quantity: 1, note: '' });
    this.showMovementForm.set(true);
  }

  protected closeMovementForm(): void {
    if (!this.isSubmitting()) {
      this.showMovementForm.set(false);
    }
  }

  protected registerMovement(): void {
    if (!this.canManage()) {
      return;
    }
    const product = this.selectedProduct();

    if (!product || this.movementForm.invalid) {
      this.movementForm.markAllAsTouched();
      return;
    }

    const value = this.movementForm.getRawValue();
    const request: CreateStockMovementRequest = {
      type: value.type,
      quantity: Number(value.quantity),
      note: value.note.trim() || null,
    };

    this.isSubmitting.set(true);
    this.api
      .registerMovement(product.id, request)
      .pipe(finalize(() => this.isSubmitting.set(false)))
      .subscribe({
        next: (movement) => {
          const action = movement.type === 'Entry' ? 'Entrada' : 'Saída';
          this.showMovementForm.set(false);
          this.notify('success', `${action} registrada. Novo saldo: ${movement.resultingBalance}.`);
          this.refreshAll();
        },
        error: (error: HttpErrorResponse) => {
          this.notify('error', this.readError(error));
        },
      });
  }

  protected openHistory(product: Product): void {
    this.selectedProduct.set(product);
    this.movements.set([]);
    this.showHistory.set(true);
    this.isLoadingMovements.set(true);

    this.api
      .getMovements(product.id)
      .pipe(finalize(() => this.isLoadingMovements.set(false)))
      .subscribe({
        next: (movements) => this.movements.set(movements),
        error: (error: HttpErrorResponse) => {
          this.notify('error', this.readError(error));
          this.showHistory.set(false);
        },
      });
  }

  protected closeHistory(): void {
    this.showHistory.set(false);
  }

  protected openReports(): void {
    this.report.set(null);
    this.showReports.set(true);
    this.isLoadingReport.set(true);
    this.api
      .getReportOverview()
      .pipe(finalize(() => this.isLoadingReport.set(false)))
      .subscribe({
        next: (report) => this.report.set(report),
        error: (error: HttpErrorResponse) => {
          this.notify('error', this.readError(error, 'Não foi possível carregar os relatórios.'));
          this.showReports.set(false);
        },
      });
  }

  protected closeReports(): void {
    if (!this.isExportingReport()) {
      this.showReports.set(false);
    }
  }

  protected exportReport(): void {
    this.isExportingReport.set(true);
    this.api
      .downloadInventoryReport()
      .pipe(finalize(() => this.isExportingReport.set(false)))
      .subscribe({
        next: (file) => {
          const url = URL.createObjectURL(file);
          const link = document.createElement('a');
          link.href = url;
          link.download = `stockflow-inventario-${new Date().toISOString().slice(0, 10)}.csv`;
          link.click();
          URL.revokeObjectURL(url);
          this.notify('success', 'Relatório CSV exportado com sucesso.');
        },
        error: (error: HttpErrorResponse) => this.notify('error', this.readError(error)),
      });
  }

  protected formatCurrency(value: number): string {
    return new Intl.NumberFormat('pt-BR', {
      style: 'currency',
      currency: 'BRL',
    }).format(value);
  }

  protected formatDate(value: string): string {
    return new Intl.DateTimeFormat('pt-BR', {
      dateStyle: 'short',
      timeStyle: 'short',
    }).format(new Date(value));
  }

  protected productInitials(name: string): string {
    return name
      .split(' ')
      .filter(Boolean)
      .slice(0, 2)
      .map((word) => word[0])
      .join('')
      .toUpperCase();
  }

  protected roleLabel(role: UserRole): string {
    return role === 'Manager' ? 'Gestor' : 'Visitante';
  }

  @HostListener('document:keydown.escape')
  protected closeTopLayer(): void {
    if (this.showReports()) {
      this.closeReports();
    } else if (this.showCatalog()) {
      this.closeCatalog();
    } else if (this.showProductForm()) {
      this.closeProductForm();
    } else if (this.showMovementForm()) {
      this.closeMovementForm();
    } else if (this.showHistory()) {
      this.closeHistory();
    }
  }

  private notify(type: Toast['type'], message: string): void {
    this.toast.set({ type, message });
    window.clearTimeout(this.toastTimer);
    this.toastTimer = window.setTimeout(() => this.toast.set(null), 4500);
  }

  private restoreSession(): void {
    this.api
      .getCurrentUser()
      .pipe(finalize(() => this.authChecked.set(true)))
      .subscribe({
        next: (user) => {
          this.currentUser.set(user);
          this.refreshAll();
        },
        error: (error: HttpErrorResponse) => {
          if (error.status !== 401) {
            this.authError.set(this.readError(error, 'Não foi possível verificar a sessão.'));
          }
        },
      });
  }

  private clearSession(message: string | null = null): void {
    this.currentUser.set(null);
    this.products.set([]);
    this.categories.set([]);
    this.suppliers.set([]);
    this.movements.set([]);
    this.report.set(null);
    this.selectedProduct.set(null);
    this.showProductForm.set(false);
    this.showMovementForm.set(false);
    this.showHistory.set(false);
    this.showReports.set(false);
    this.showCatalog.set(false);
    this.authError.set(message);
  }

  private readError(
    error: HttpErrorResponse,
    fallback = 'Não foi possível concluir a operação.',
  ): string {
    if (typeof error.error?.detail === 'string') {
      return error.error.detail;
    }

    if (error.status === 0) {
      return 'A API está desligada. Inicie o projeto .NET e tente novamente.';
    }

    if (error.status === 403) {
      return 'Seu perfil não tem permissão para realizar esta operação.';
    }

    if (error.status === 401) {
      return 'Sua sessão expirou. Entre novamente.';
    }

    return fallback;
  }
}
