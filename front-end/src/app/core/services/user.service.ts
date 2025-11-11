import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';

import { environment } from '../../../environments/environment';
import { UserResponse } from '../models/user.models';

interface SearchRequest {
  username: string;
}

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private readonly http = inject(HttpClient);
  

  getUsers(username: string): Observable<UserResponse[]> {
    const params = new HttpParams().set('username', username);
    return this.http.get<UserResponse[]>(
      `${environment.apiUrl}/user`,
      {params}
    );
  }
}
