import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class BaseApiService {
  // App Service
  protected baseUrl = 'https://password-generator-ctf8a4fpandwb9f5.polandcentral-01.azurewebsites.net/api';

  // Local
  //protected baseUrl = 'https://localhost:5091/api';

  // VM
  //protected baseUrl = 'https://passwordgenerator.polandcentral.cloudapp.azure.com/api';
}