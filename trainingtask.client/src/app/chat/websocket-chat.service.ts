import { Injectable } from '@angular/core';
import { Observable, Subject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class WebsocketChatService {
  private ws?: WebSocket;
  private messageSubject = new Subject<string>();

  connect(): void {
    this.ws = new WebSocket(`'wss://localhost:7017/ws/chat'`); // Adjust port if needed

    this.ws.onopen = () => {
      console.log('WebSocket connected');
    };

    this.ws.onmessage = (event) => {
      console.log('Received:', event.data);
      this.messageSubject.next(event.data);
    };

    this.ws.onclose = () => {
      console.log('WebSocket closed');
    };

    this.ws.onerror = (err) => {
      console.error('WebSocket error:', err);
    };
  }

  sendMessage(sessionId: string, message: string, jsonCreds: string): void {
    if (this.ws && this.ws.readyState === WebSocket.OPEN) {
      this.ws.send(JSON.stringify({ SessionId: sessionId, Message: message, JsonCreds: jsonCreds }));
      console.log('Message sent:', { SessionId: sessionId, Message: message, JsonCreds: jsonCreds });
    } else {
      console.error('WebSocket is not open.');
    }
  }

  onMessage(): Observable<string> {
    return this.messageSubject.asObservable();
  }

  close(): void {
    this.ws?.close();
  }
}
