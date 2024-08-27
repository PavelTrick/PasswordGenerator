import { HttpClient, HttpErrorResponse, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, map, Observable, of } from 'rxjs';
import { LoginResponse } from '../models/login-response.model';
import { LoginModel } from '../models/login.model';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private baseUrl = 'https://localhost:5091/api/account';

  constructor(private http: HttpClient) { }

  login(login: string, password: string): Observable<any> {
    const headers = new HttpHeaders({ 'Content-Type': 'application/json' });
    const loginModel = new LoginModel(login, password);

    return this.http.post<LoginResponse>(`${this.baseUrl}/login`, loginModel, { headers, withCredentials: true }).pipe(
      map(response => {
        if (response) {
          localStorage.setItem('authToken', response.accessToken);
          return of(true);
        }
        return of(false);
      }),
      catchError((error: HttpErrorResponse) => {
        console.log(error);
        if (error.status) {
          alert(`Backend returned code ${error.status} ${error.error.title}`);
        } else {
          alert('An unexpected error occurred. Please try again later.');
        }
        return of(false);
      })
    )
  }

  isAuthenticated(): boolean {
    const token = localStorage.getItem('authToken');
    return !!token;
  }

  logout(): Observable<any> {
    localStorage.removeItem('authToken');
    return this.http.post<any>(`${this.baseUrl}/logout`, { withCredentials: true })
  }

  register(login: string, password: string): Observable<boolean> {
    const headers = new HttpHeaders({ 'Content-Type': 'application/json' });
    const loginModel = new LoginModel(login, password);

    return this.http.post<LoginResponse>(`${this.baseUrl}/register`, loginModel, { headers, withCredentials: true }).pipe(
      map(response => {
        if (response) {
          localStorage.setItem('authToken', response.accessToken);
          return true;
        }
        return false;
      }),
      catchError((error: HttpErrorResponse) => {
        if (error.status) {
          alert(`Backend returned code ${error.status} ${error.statusText}`);
        } else {
          alert('An unexpected error occurred. Please try again later.');
        }
        return of(false);
      })
    );
  }
}