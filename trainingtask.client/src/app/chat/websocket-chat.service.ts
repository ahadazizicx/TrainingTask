import { Injectable } from '@angular/core';
import { Observable, Subject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class WebsocketChatService {
  private ws?: WebSocket;
  private messageSubject = new Subject<string>();

  private reconnectAttempts = 0;
  private maxReconnectAttempts = 25;

  connect(): void {
    this.ws = new WebSocket('wss://localhost:7017/ws/chat'); // Adjust port if needed

    this.ws.onopen = () => {
      console.log('WebSocket connected');
      this.reconnectAttempts = 0; 
    };

    this.ws.onmessage = (event) => {
      console.log('Received:', event.data);
      this.messageSubject.next(event.data);
    };

    this.ws.onclose = () => {
      console.log('WebSocket closed');
      this.reconnect();
    };

    this.ws.onerror = (err) => {
      console.error('WebSocket error:', err);
    };
  }

  sendMessage(sessionId: string, message: string, jsonCreds: string): boolean {
    if (this.ws && this.ws.readyState === WebSocket.OPEN) {
      this.ws.send(JSON.stringify({ SessionId: sessionId, Message: message, JsonCreds: jsonCreds }));
      console.log('Message sent:', { SessionId: sessionId, Message: message, JsonCreds: jsonCreds });
      return true;
    } else {
      console.error('WebSocket is not open.');
      return false;
    }
  }

  onMessage(): Observable<string> {
    return this.messageSubject.asObservable();
  }

  close(): void {
    this.ws?.close();
  }

  private reconnect(): void {
    if (this.reconnectAttempts < this.maxReconnectAttempts) {
      setTimeout(() => {
        console.log(`Reconnection attempt #${this.reconnectAttempts + 1}`);
        this.reconnectAttempts++;
        this.connect();
      }, 2000); // Wait 2 seconds before trying to reconnect
    } else {
      console.warn('Max reconnection attempts reached');
    }
  }


}
