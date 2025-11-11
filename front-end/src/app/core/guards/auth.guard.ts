import { Injectable, inject } from '@angular/core';
import {
  ActivatedRouteSnapshot,
  CanActivate,
  Router,
  RouterStateSnapshot,
  UrlTree
} from '@angular/router';
import { Observable } from 'rxjs';

import { AuthService } from '../services/auth.service';

@Injectable({ providedIn: 'root' })
export class AuthGuard implements CanActivate {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  canActivate(route: ActivatedRouteSnapshot): boolean | UrlTree {
    const isAuthenticated = this.auth.isAuthenticated();
    const allowIfLoggedOut = route.data['allowIfLoggedOut'] === true;

    if (isAuthenticated && allowIfLoggedOut) {
      // landing page per utenti già loggati → redirect a friends
      return this.router.parseUrl('/friends');
    }

    if (!isAuthenticated && !allowIfLoggedOut) {
      // route protetta ma utente non loggato
      return this.router.parseUrl('/');
    }

    return true;
  }
}

