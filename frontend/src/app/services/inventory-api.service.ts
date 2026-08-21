import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
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
  Supplier,
  UpdateProductRequest,
  UserSession,
} from '../models/inventory.models';

@Injectable({ providedIn: 'root' })
export class InventoryApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api';

  login(request: LoginRequest): Observable<UserSession> {
    return this.http.post<UserSession>(`${this.baseUrl}/auth/login`, request);
  }

  getCurrentUser(): Observable<UserSession> {
    return this.http.get<UserSession>(`${this.baseUrl}/auth/me`);
  }

  logout(): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/auth/logout`, {});
  }

  getProducts(search = '', lowStockOnly = false): Observable<Product[]> {
    let params = new HttpParams().set('lowStockOnly', lowStockOnly);

    if (search.trim()) {
      params = params.set('search', search.trim());
    }

    return this.http.get<Product[]>(`${this.baseUrl}/products`, { params });
  }

  createProduct(request: CreateProductRequest): Observable<Product> {
    return this.http.post<Product>(`${this.baseUrl}/products`, request);
  }

  updateProduct(id: number, request: UpdateProductRequest): Observable<Product> {
    return this.http.put<Product>(`${this.baseUrl}/products/${id}`, request);
  }

  getCategories(): Observable<Category[]> {
    return this.http.get<Category[]>(`${this.baseUrl}/categories`);
  }

  createCategory(request: CreateCategoryRequest): Observable<Category> {
    return this.http.post<Category>(`${this.baseUrl}/categories`, request);
  }

  getSuppliers(): Observable<Supplier[]> {
    return this.http.get<Supplier[]>(`${this.baseUrl}/suppliers`);
  }

  createSupplier(request: CreateSupplierRequest): Observable<Supplier> {
    return this.http.post<Supplier>(`${this.baseUrl}/suppliers`, request);
  }

  getMovements(productId: number): Observable<StockMovement[]> {
    return this.http.get<StockMovement[]>(`${this.baseUrl}/products/${productId}/movements`);
  }

  registerMovement(
    productId: number,
    request: CreateStockMovementRequest,
  ): Observable<StockMovement> {
    return this.http.post<StockMovement>(
      `${this.baseUrl}/products/${productId}/movements`,
      request,
    );
  }

  getSummary(): Observable<InventorySummary> {
    return this.http.get<InventorySummary>(`${this.baseUrl}/dashboard/summary`);
  }

  getReportOverview(): Observable<ReportOverview> {
    return this.http.get<ReportOverview>(`${this.baseUrl}/reports/overview`);
  }

  downloadInventoryReport(): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/reports/inventory.csv`, { responseType: 'blob' });
  }
}
