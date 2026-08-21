export type StockMovementType = 'Entry' | 'Exit';
export type UserRole = 'Viewer' | 'Manager';

export interface UserSession {
  name: string;
  email: string;
  role: UserRole;
  canManage: boolean;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface Product {
  id: number;
  sku: string;
  name: string;
  unitPrice: number;
  quantityInStock: number;
  minimumStock: number;
  maximumStock: number;
  isLowStock: boolean;
  suggestedReorderQuantity: number;
  stockValue: number;
  categoryId: number | null;
  categoryName: string | null;
  supplierId: number | null;
  supplierName: string | null;
  isActive: boolean;
  createdAtUtc: string;
}

export interface StockMovement {
  id: number;
  productId: number;
  type: StockMovementType;
  quantity: number;
  resultingBalance: number;
  note: string | null;
  performedByName: string;
  performedByEmail: string;
  createdAtUtc: string;
}

export interface ReportOverview {
  generatedAtUtc: string;
  summary: InventorySummary;
  categories: CategoryReportItem[];
  lowStockProducts: LowStockReportItem[];
  recentMovements: MovementReportItem[];
}

export interface CategoryReportItem {
  categoryName: string;
  productCount: number;
  totalUnits: number;
  totalStockValue: number;
}

export interface LowStockReportItem {
  productId: number;
  sku: string;
  productName: string;
  quantityInStock: number;
  minimumStock: number;
  maximumStock: number;
  suggestedReorderQuantity: number;
}

export interface MovementReportItem {
  id: number;
  productId: number;
  sku: string;
  productName: string;
  type: StockMovementType;
  quantity: number;
  resultingBalance: number;
  performedByName: string;
  createdAtUtc: string;
}

export interface InventorySummary {
  totalProducts: number;
  lowStockProducts: number;
  totalUnits: number;
  totalStockValue: number;
}

export interface CreateProductRequest {
  sku: string;
  name: string;
  unitPrice: number;
  minimumStock: number;
  maximumStock: number;
  categoryId: number | null;
  supplierId: number | null;
}

export interface UpdateProductRequest {
  name: string;
  unitPrice: number;
  minimumStock: number;
  maximumStock: number;
  categoryId: number | null;
  supplierId: number | null;
}

export interface Category {
  id: number;
  name: string;
  isActive: boolean;
  createdAtUtc: string;
}

export interface Supplier {
  id: number;
  name: string;
  contactName: string | null;
  email: string | null;
  phone: string | null;
  isActive: boolean;
  createdAtUtc: string;
}

export interface CreateCategoryRequest {
  name: string;
}

export interface CreateSupplierRequest {
  name: string;
  contactName: string | null;
  email: string | null;
  phone: string | null;
}

export interface CreateStockMovementRequest {
  type: StockMovementType;
  quantity: number;
  note: string | null;
}
