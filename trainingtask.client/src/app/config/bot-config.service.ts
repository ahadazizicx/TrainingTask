import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { BotConfig } from './bot-config.model';

@Injectable({ providedIn: 'root' })
export class BotConfigService {
  private apiUrl = 'https://localhost:7017/api/BotConfig'; // TODO: Replace with your real API endpoint

  constructor(private http: HttpClient) {}

  getBots(): Observable<{ success: boolean; data: BotConfig[] }> {
    return this.http.get<{ success: boolean; data: BotConfig[] }>(this.apiUrl, { withCredentials: true });
  }

  getBotById(id: string): Observable<{ success: boolean; data: BotConfig }> {
    return this.http.get<{ success: boolean; data: BotConfig }>(`${this.apiUrl}/${id}`, { withCredentials: true });
  }

  updateBot(bot: BotConfig): Observable<any> {
    return this.http.put(`${this.apiUrl}/${bot.id}`, bot, { withCredentials: true });
  }

  deleteBot(id: string): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`, { withCredentials: true });
  }

  createBot(bot: BotConfig): Observable<any> {
    return this.http.post(`${this.apiUrl}/create`, bot, { withCredentials: true });
  }
}
