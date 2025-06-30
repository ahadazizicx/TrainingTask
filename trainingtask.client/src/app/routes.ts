import { Routes } from '@angular/router';
import { ChatComponent } from './chat/chat.component';
import { ConfigComponent } from './config/config.component';
import { LoginComponent } from './login/login.component';

export const routes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  { path: 'chat', component: ChatComponent },
  { path: 'config', component: ConfigComponent },
  { path: 'login', component: LoginComponent }
];
