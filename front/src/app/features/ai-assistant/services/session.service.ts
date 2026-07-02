import { Injectable } from '@angular/core';

// Utility for generating UUIDv4
function uuidv4() {
  return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
    const r = Math.random() * 16 | 0, v = c === 'x' ? r : (r & 0x3 | 0x8);
    return v.toString(16);
  });
}

@Injectable({ providedIn: 'root' })
export class SessionService {
  private readonly STORAGE_KEY = 'ai_guest_sessionId';

  getSessionId(): string {
    let sess = localStorage.getItem(this.STORAGE_KEY);
    if (!sess) {
      sess = uuidv4();
      localStorage.setItem(this.STORAGE_KEY, sess);
    }
    return sess;
  }

  newSessionId(): string {
    const sess = uuidv4();
    localStorage.setItem(this.STORAGE_KEY, sess);
    return sess;
  }

  clearSessionId() {
    localStorage.removeItem(this.STORAGE_KEY);
  }
}
