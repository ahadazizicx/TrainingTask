import { Component, OnInit, Input } from '@angular/core';
import { WebsocketChatService } from './websocket-chat.service';
import { BotConfigService } from '../config/bot-config.service';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { NgbToastModule } from '@ng-bootstrap/ng-bootstrap';
import { ActivatedRoute, Router } from '@angular/router';

import { LucideAngularModule, SendHorizontal } from 'lucide-angular';
import { v4 as uuidv4 } from 'uuid';
import { BotConfig } from '../config/bot-config.model';

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

  sessionid: string = ''; 
  botId: string | null = null;

  activeBot: BotConfig = {
    id: '',
    botName: '',
    jsonCreds: '',
    languageCode: '',
    userId: '',
  }
    
  loader: boolean = false;

  constructor(
    private wsService: WebsocketChatService,
    private botConfigService: BotConfigService,
    private route: ActivatedRoute,
    private router: Router
  ) { }

  ngOnInit() {
    this.botId = this.route.snapshot.paramMap.get('id');
    if (!this.botId) {
      console.error('No bot ID provided in route parameters');
      this.router.navigate(['/config']);
      return;
    }

    this.loader = true;
    // get bot config from service
    this.botConfigService.getBotById(this.botId).subscribe({
      next: res => {
        if (res.success) {
          console.log('Bot config:', res.data);
          this.activeBot = res.data; 
        } else {
          console.error('Failed to fetch bot config');
          this.router.navigate(['/config']);
        }
      },
      error: err => {
        if (err.status === 401) {
          this.router.navigate(['/login']);
        } else {
          console.error('Error fetching bot config:', err);
          this.router.navigate(['/config']);
        }
      }
    });
    // this.BotName = 
    this.loader = false;

    // Connect to WebSocket service
    this.wsService.connect();
    this.wsService.onMessage().subscribe((msg: string) => {
      this.receiveMessage(msg);
    });
    this.sessionid = uuidv4();
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

    const SessionId = this.sessionid;  
    const Message = this.message;
    const jsonCreds = this.activeBot.jsonCreds;
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
