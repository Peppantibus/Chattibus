import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

type AuthModalType = 'login' | 'register' | null;

@Injectable({
  providedIn: 'root'
})
export class ModalService {
  private readonly authModalSubject = new BehaviorSubject<AuthModalType>(null);
  readonly authModal$ = this.authModalSubject.asObservable();

  openLogin(): void {
    this.authModalSubject.next('login');
  }

  openRegister(): void {
    this.authModalSubject.next('register');
  }

  closeAuthModal(): void {
    this.authModalSubject.next(null);
  }
}
