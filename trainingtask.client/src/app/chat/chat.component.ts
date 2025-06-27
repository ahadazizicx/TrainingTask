import { Component, OnInit, Input } from '@angular/core';
import { WebsocketChatService } from './websocket-chat.service';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

import { LucideAngularModule, SendHorizontal } from 'lucide-angular';

@Component({
  selector: 'app-chat',
  standalone: true,
  imports: [FormsModule, CommonModule, LucideAngularModule],
  templateUrl: './chat.component.html',
  styleUrls: ['./chat.component.css']
})
export class ChatComponent implements OnInit {
  // @Input() sessionId: string = 'default-session-id';
  message: string = '';
  messages: Array<any> = [];
  readonly SendHorizontal = SendHorizontal;

  constructor(private wsService: WebsocketChatService) { }

  ngOnInit() {
    this.wsService.connect();
    this.wsService.onMessage().subscribe((msg: string) => {
      this.receiveMessage(msg);
    });
  }

  send() {
    if (!this.message.trim()) {
      return; 
    }
    console.log('Sending message:', this.message);
    const SessionId = '122';
    const Message = this.message;
    //const jsonCreds = '{ "project_id": "your-project-id", ... }'; // Paste your service account JSON here
    const jsonCreds = ''; // Use an empty string if you don't have credentials to send
    this.wsService.sendMessage(SessionId, Message, jsonCreds);

    this.messages.push({
      type: "sent",
      content: this.message,
      timestamp: new Date().toLocaleTimeString()
    });

    this.message = ''; 
  }

  receiveMessage(msg: any) {
    const jsonmsg = JSON.parse(msg);

    this.messages.push({
      type: "received",
      content: jsonmsg.fulfillmentText,
      intent: jsonmsg.intentName,
      timestamp: new Date().toLocaleTimeString()
    });
    // Optionally, update the UI or perform other actions here
    // console.log('Received message:', msg);
    console.log('Updated messages:', this.messages);
  }

  close() {
    this.wsService.close();
  }
}
