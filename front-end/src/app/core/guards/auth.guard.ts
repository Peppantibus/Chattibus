import { Injectable } from '@angular/core';
import {
  CanActivate,
  Router,
  ActivatedRouteSnapshot
} from '@angular/router';

import { AuthService } from '../services/auth.service';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {

  constructor(
    private readonly authService: AuthService,
    private readonly router: Router
  ) {}

  canActivate(route: ActivatedRouteSnapshot): boolean {
    const allowIfLoggedOut = route.data['allowIfLoggedOut'] === true;
    const isAuth = this.authService.isAuthenticated();

    // 1. Se è una pagina guest-friendly → permetti SEMPRE
    if (allowIfLoggedOut) {
      return true;
    }

    // 2. Se richiede autenticazione → verifica token
    if (isAuth) {
      return true;
    }

    // 3. Utente non autenticato su pagina protetta → redirect landing
    this.router.navigate(['/'], {
      queryParams: { showLogin: true }
    });

    return false;
  }
}
