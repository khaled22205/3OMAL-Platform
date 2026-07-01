import { Injectable, inject } from '@angular/core';
import { MockDataService } from './mock-data.service';
import { Booking } from '../models/interfaces';

@Injectable({ providedIn: 'root' })
export class BookingService {
  private mockData = inject(MockDataService);
  bookings = this.mockData.bookings;

  getBookingsForUser(userId: string, role: 'client' | 'worker') {
    return this.bookings();
  }

  createBooking(bookingData: Omit<Booking, 'id' | 'status' | 'createdAt'>) {
    return this.mockData.addBooking(bookingData);
  }

  updateStatus(bookingId: string, status: Booking['status']) {
    this.mockData.updateBookingStatus(bookingId, status);
  }
}
