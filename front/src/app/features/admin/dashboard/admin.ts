import { Component, inject, signal, computed, OnInit, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { AuthService } from '../../../core/services/auth.service';
import { AdminService } from '../../../core/services/admin.service';
import { ToastService } from '../../../core/services/toast.service';
import { DashboardStatsResponse, UserManagementResponse } from '../../../core/models/admin.models';

type AdminTab = 'overview' | 'users' | 'reports';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './admin.html',
  styleUrl: './admin.css',
})
export default class AdminDashboard implements OnInit {
  private readonly auth = inject(AuthService);
  private readonly adminService = inject(AdminService);
  private readonly toast = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);

  readonly user = this.auth.user;

  activeTab = signal<AdminTab>('overview');
  loading = signal(true);
  error = signal<string | null>(null);

  stats = signal<DashboardStatsResponse | null>(null);

  users = signal<UserManagementResponse[]>([]);
  userPage = signal(1);
  userTotal = signal(0);
  userSearch = signal('');
  userRoleFilter = signal('');
  userPageSize = 10;

  totalUserPages = computed(() => Math.max(1, Math.ceil(this.userTotal() / this.userPageSize)));

  editUser = signal<UserManagementResponse | null>(null);
  editEmail = '';
  editPhone = '';
  editRole = '';

  ngOnInit() {
    this.loadStats();
    this.loadUsers();
  }

  loadStats() {
    this.adminService
      .getDashboard()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (data) => {
          this.stats.set(data);
          this.loading.set(false);
        },
        error: () => {
          this.error.set('Failed to load dashboard stats');
          this.loading.set(false);
          this.toast.show('Failed to load dashboard statistics', 'error');
        },
      });
  }

  private loadUsers() {
    this.adminService
      .getUsers(this.userPage(), this.userPageSize, this.userRoleFilter() || undefined, this.userSearch() || undefined)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (result) => {
          this.users.set(result.items);
          this.userTotal.set(result.totalCount);
        },
        error: () => this.toast.show('Failed to load users', 'error'),
      });
  }

  onSearchUsers() {
    this.userPage.set(1);
    this.loadUsers();
  }

  onPrevUserPage() {
    if (this.userPage() > 1) {
      this.userPage.update((p) => p - 1);
      this.loadUsers();
    }
  }

  onNextUserPage() {
    if (this.userPage() < this.totalUserPages()) {
      this.userPage.update((p) => p + 1);
      this.loadUsers();
    }
  }

  onFilterByRole(role: string) {
    this.userRoleFilter.set(role);
    this.userPage.set(1);
    this.loadUsers();
  }

  onEditUser(u: UserManagementResponse) {
    this.editUser.set(u);
    this.editEmail = u.email;
    this.editPhone = u.phoneNumber || '';
    this.editRole = u.roles[0] || '';
  }

  onCancelEdit() {
    this.editUser.set(null);
  }

  onSaveUser() {
    const u = this.editUser();
    if (!u) return;
    this.adminService
      .updateUser(u.id, { email: this.editEmail, phoneNumber: this.editPhone || undefined, role: this.editRole || undefined })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.toast.show(`User ${u.email} updated`, 'success');
          this.editUser.set(null);
          this.loadUsers();
        },
        error: () => this.toast.show('Failed to update user', 'error'),
      });
  }

  onLockUser(u: UserManagementResponse) {
    this.adminService
      .lockUser(u.id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.toast.show(`User ${u.email} locked`, 'info');
          this.loadUsers();
        },
        error: () => this.toast.show('Failed to lock user', 'error'),
      });
  }

  onDeleteUser(u: UserManagementResponse) {
    if (!confirm(`Delete user ${u.email}? This action cannot be undone.`)) return;
    this.adminService
      .deleteUser(u.id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.toast.show(`User ${u.email} deleted`, 'info');
          this.loadUsers();
        },
        error: () => this.toast.show('Failed to delete user', 'error'),
      });
  }

  onExportUsers() {
    this.adminService
      .exportUsers()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (blob) => this.downloadBlob(blob, 'users-export.csv'),
        error: () => this.toast.show('Failed to export users', 'error'),
      });
  }

  onExportBookings() {
    this.adminService
      .exportBookings()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (blob) => this.downloadBlob(blob, 'bookings-export.csv'),
        error: () => this.toast.show('Failed to export bookings', 'error'),
      });
  }

  private downloadBlob(blob: Blob, filename: string) {
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = filename;
    a.click();
    window.URL.revokeObjectURL(url);
  }

  getMaxMonthlyCount(): number {
    const months = this.stats()?.monthlyBookings;
    if (!months || months.length === 0) return 1;
    return Math.max(...months.map((m) => m.count), 1);
  }

  logout(): void {
    this.auth.logout();
  }
}
