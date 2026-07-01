import { Component, inject } from '@angular/core';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-admin-dashboard',
  imports: [],
  templateUrl: './admin.html',
  styleUrl: './admin.css',
})
export default class AdminDashboard {
  private readonly auth = inject(AuthService);
  readonly user = this.auth.user;

  logout(): void {
    this.auth.logout();
  }
}
