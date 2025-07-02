import { HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppComponent } from './app.component';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { FormsModule } from '@angular/forms';
import { ChatComponent } from './chat/chat.component';
import { LoginComponent } from './login/login.component';
import { SignupComponent } from './signup/signup.component';

@NgModule({
  declarations: [
    // ChatComponent
  
    LoginComponent,
    SignupComponent
  ],
  imports: [
    BrowserModule, HttpClientModule, FormsModule, ChatComponent
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
