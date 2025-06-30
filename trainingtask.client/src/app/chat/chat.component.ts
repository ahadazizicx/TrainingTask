import { Component, OnInit, Input } from '@angular/core';
import { WebsocketChatService } from './websocket-chat.service';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { NgbToastModule } from '@ng-bootstrap/ng-bootstrap';

import { LucideAngularModule, SendHorizontal } from 'lucide-angular';

@Component({
  selector: 'app-chat',
  standalone: true,
  imports: [FormsModule, CommonModule, LucideAngularModule, NgbToastModule],
  templateUrl: './chat.component.html',
  styleUrls: ['./chat.component.css']
})
export class ChatComponent implements OnInit {
  // @Input() sessionId: string = 'default-session-id';
  message: string = '';
  messages: Array<any> = [];
  readonly SendHorizontal = SendHorizontal;

  showNoInputToast = false;
  showInvalidInputToast = false;
  showSendQuickReplyToast = false;
  showSuccessToast = false;
  showMessageSendFailToast = false;
  toastTimeout: any;

  constructor(private wsService: WebsocketChatService) { }

  ngOnInit() {
    this.wsService.connect();
    this.wsService.onMessage().subscribe((msg: string) => {
      this.receiveMessage(msg);
    });
  }

  // Helper to show a toast by id
  showToast(type: string) {
    // Hide all toasts first
    this.showNoInputToast = false;
    this.showInvalidInputToast = false;
    this.showSendQuickReplyToast = false;
    this.showSuccessToast = false;
    this.showMessageSendFailToast = false;
    if (this.toastTimeout) {
      clearTimeout(this.toastTimeout);
    }
    switch (type) {
      case 'noInputToast':
        this.showNoInputToast = true;
        break;
      case 'invalidInputToast':
        this.showInvalidInputToast = true;
        break;
      case 'sendQuickReplyToast':
        this.showSendQuickReplyToast = true;
        break;
      case 'successToast':
        this.showSuccessToast = true;
        break;
      case 'messageSendFail':
        this.showMessageSendFailToast = true;
        break;
    }
    // Hide toast after 2 seconds
    this.toastTimeout = setTimeout(() => {
      this.showNoInputToast = false;
      this.showInvalidInputToast = false;
      this.showSendQuickReplyToast = false;
      this.showSuccessToast = false;
      this.showMessageSendFailToast = false;
    }, 2000);
  }

  send() {
    if (!this.message.trim()) {
      this.showToast('noInputToast');
      return;
    }
    // Example validation: only allow alphanumeric and spaces
    const isValid = /^[\w\s.,!?'-]+$/.test(this.message.trim());
    if (!isValid) {
      this.showToast('invalidInputToast');
      return;
    }
    // Optionally show sending toast (if you have quick reply logic, call it there)
    // this.showToast('sendQuickReplyToast');

    console.log('Sending message:', this.message);
    const SessionId = '122';
    const Message = this.message;
    const jsonCreds = '';
    const sent = this.wsService.sendMessage(SessionId, Message, jsonCreds);
    if (sent) {
      this.messages.push({
        type: "sent",
        content: this.message,
        timestamp: new Date().toLocaleTimeString()
      });
      this.message = '';
      this.showToast('successToast');
    } else {
      this.showToast('messageSendFail');
    }
  }

  receiveMessage(msg: any) {
    try {
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
    } catch (error) {
      this.showToast('messageSendFail');
    }
  }

  close() {
    this.wsService.close();
  }
}
