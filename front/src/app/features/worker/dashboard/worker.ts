import { Component, inject } from '@angular/core';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-worker-dashboard',
  imports: [],
  templateUrl: './worker.html',
  styleUrl: './worker.css',
})
export default class WorkerDashboard {
  private readonly auth = inject(AuthService);
  readonly user = this.auth.user;

  logout(): void {
    this.auth.logout();
  }
}
