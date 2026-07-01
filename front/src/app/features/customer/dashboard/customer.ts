import { Component, inject } from '@angular/core';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-customer-dashboard',
  imports: [],
  templateUrl: './customer.html',
  styleUrl: './customer.css',
})
export default class CustomerDashboard {
  private readonly auth = inject(AuthService);
  readonly user = this.auth.user;

  logout(): void {
    this.auth.logout();
  }
}
