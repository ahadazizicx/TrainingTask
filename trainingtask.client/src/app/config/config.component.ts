  
import { Component, OnInit } from '@angular/core';
import { BotConfig } from './bot-config.model';
import { BotConfigService } from './bot-config.service';
import { Router } from '@angular/router';

import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { NgbToastModule } from '@ng-bootstrap/ng-bootstrap';

@Component({
  selector: 'app-config',
  standalone: true,
  imports: [CommonModule, FormsModule, NgbToastModule],
  templateUrl: './config.component.html',
  styleUrls: ['./config.component.css'],
  providers: [BotConfigService]
})
export class ConfigComponent implements OnInit {
  bots: BotConfig[] = [];
  selectedBot: BotConfig | null = null;
  editedBot: Partial<BotConfig> = {};
  newBot: Partial<BotConfig> = { botName: '', jsonCreds: '', languageCode: '' };

  showUpdateFailToast = false;
  showUpdateSuccessToast = false;
  showNewBotSuccessToast = false;
  showNewBotFailToast = false;
  showDeleteSuccessToast = false;
  showDeleteFailToast = false;
  toastTimeout: any;

  showCreateBotForm: boolean = false;
  constructor(private botConfigService: BotConfigService, private router: Router) {}

  ngOnInit() {
    this.fetchBots();
  }

  loader: boolean = true;

  fetchBots() {
    this.loader = true;
    this.botConfigService.getBots().subscribe({
      next: res => {
        if (res.success) {
          this.bots = res.data;
          if (this.bots.length) {
            this.selectBot(this.bots[0]);
            this.showCreateBotForm = false;
          } else {
            this.selectedBot = null;
            this.showCreateBotForm = true;
          }
        }
      },
      error: err => {
        if (err.status === 401) {
          this.router.navigate(['/login']);
        }
      }
    });
    this.loader = false;
  }

  selectBot(bot: BotConfig) {
    this.selectedBot = bot;
    console.log('Selected Bot: ', this.selectedBot);
    this.editedBot = { ...bot };
  }

  saveBot() {
    if (!this.selectedBot) return;
    const updatedBot: BotConfig = { ...this.selectedBot, ...this.editedBot } as BotConfig;
    this.botConfigService.updateBot(updatedBot).subscribe({
      next: () => {
        this.fetchBots();
        this.ShowToast('updateSuccess');
      },
      error: err => {
        if (err.status === 401) {
          this.router.navigate(['/login']);
        }
      }
    });
  }

  deleteBot() {
    if (!this.selectedBot) return;
    this.botConfigService.deleteBot(this.selectedBot.id).subscribe({
      next: () => {
        this.fetchBots();
        this.ShowToast('deleteSuccess');
      },
      error: err => {
        if (err.status === 401) {
          this.router.navigate(['/login']);
        }
      }
    });

  }

  createBot() {
    if (!this.newBot.botName || !this.newBot.jsonCreds || !this.newBot.languageCode) return;
    this.botConfigService.createBot(this.newBot as BotConfig).subscribe({
      next: () => {
        this.newBot = { botName: '', jsonCreds: '', languageCode: '' };
        this.fetchBots();
        this.showCreateBotForm = false;
        this.ShowToast('newBotSuccess');
      },
      error: err => {
        if (err.status === 401) {
          this.router.navigate(['/login']);
        }
      }
    });
  }

  enterChat() {
    if (!this.selectedBot) return;
    console.log('Navigating to chat with bot: ', this.selectedBot);
    this.router.navigate(['/chat', this.selectedBot.id]);
  }

  ShowToast(type: string) {
    // Hide all toasts first
    this.showUpdateFailToast = false;
    this.showUpdateSuccessToast = false;
    this.showNewBotSuccessToast = false;
    this.showNewBotFailToast = false;
    this.showDeleteSuccessToast = false;
    this.showDeleteFailToast = false;
    if (this.toastTimeout) {
      clearTimeout(this.toastTimeout);
    }

    switch (type) {
      case 'updateFail':
        this.showUpdateFailToast = true;
        break;
      case 'updateSuccess':
        this.showUpdateSuccessToast = true;
        break;
      case 'newBotSuccess':
        this.showNewBotSuccessToast = true;
        break;
      case 'newBotFail':
        this.showNewBotFailToast = true;
        break;
      case 'deleteSuccess':
        this.showDeleteSuccessToast = true;
        break;
      case 'deleteFail':
        this.showDeleteFailToast = true;
        break;
    }

    this.toastTimeout = setTimeout(() => {
      this.showUpdateFailToast = false;
      this.showUpdateSuccessToast = false;
      this.showNewBotSuccessToast = false;
      this.showNewBotFailToast = false;
      this.showDeleteSuccessToast = false;
      this.showDeleteFailToast = false;
    }, 2000);
  }
}
