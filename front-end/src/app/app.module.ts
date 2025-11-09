import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { LandingPageComponent } from './pages/landing/landing-page.component';
import { HomePageComponent } from './pages/home/home-page.component';
import { FriendsPageComponent } from './pages/friends/friends-page.component';
import { ChatPageComponent } from './pages/chat/chat-page.component';
import { HeaderComponent } from './shared/components/header/header.component';
import { LoginModalComponent } from './shared/components/modals/login/login-modal.component';
import { RegisterModalComponent } from './shared/components/modals/register/register-modal.component';
import { ChatListComponent } from './shared/components/chat-list/chat-list.component';
import { ChatWindowComponent } from './shared/components/chat-window/chat-window.component';
import { JwtInterceptor } from './core/interceptors/jwt.interceptor';

@NgModule({
  declarations: [
    AppComponent,
    LandingPageComponent,
    HomePageComponent,
    FriendsPageComponent,
    ChatPageComponent,
    HeaderComponent,
    LoginModalComponent,
    RegisterModalComponent,
    ChatListComponent,
    ChatWindowComponent
  ],
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    HttpClientModule,
    FormsModule,
    ReactiveFormsModule,
    AppRoutingModule
  ],
  providers: [
    {
      provide: HTTP_INTERCEPTORS,
      useClass: JwtInterceptor,
      multi: true
    }
  ],
  bootstrap: [AppComponent]
})
export class AppModule {}
