import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class BaseApiService {
  //protected baseUrl = 'https://password-generator-ctf8a4fpandwb9f5.polandcentral-01.azurewebsites.net/api';
  protected baseUrl = 'https://localhost:5091/api';
}