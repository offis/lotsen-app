/**
 * @license
 * Copyright (c) 2021 OFFIS e.V.. All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *
 * 1. Redistributions of source code must retain the above copyright notice, this
 *    list of conditions and the following disclaimer.
 *
 * 2. Redistributions in binary form must reproduce the above copyright notice,
 *    this list of conditions and the following disclaimer in the documentation
 *    and/or other materials provided with the distribution.
 *
 * 3. Neither the name of the copyright holder nor the names of its contributors
 *    may be used to endorse or promote products derived from this software without
 *    specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
 * FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
 * DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
 * SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
 * CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
 * OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
 * OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

import { Injectable } from '@angular/core';
import { UserModel } from './user-model';
import { Observable, BehaviorSubject } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { OfflineUserHasPasswordDto } from './offline-user-has-password-dto';

@Injectable({
  providedIn: 'root',
})
export class UserService {
  private signedInUser: UserModel = new UserModel();
  private signedInSubject: BehaviorSubject<boolean> =
    new BehaviorSubject<boolean>(false);
  private isSignedIn = false;

  public get User(): UserModel | undefined {
    return this.isSignedIn ? this.signedInUser : undefined;
  }

  constructor(private http: HttpClient) {}

  public get IsSignedIn(): Observable<boolean> {
    return this.signedInSubject;
  }

  async SignInWithPassword(
    userName: string,
    password: string,
    rememberMe: boolean,
    server: string,
    forceOnline: boolean
  ): Promise<void> {
    try {
      this.signedInUser = await this.http
        .post<UserModel>('/api/authentication/password', {
          Server: server,
          Username: userName,
          Password: password,
          RememberMe: rememberMe,
          EnforceOnline: forceOnline,
        })
        .toPromise();
      this.isSignedIn = true;
      this.signedInSubject.next(true);
    } catch (error) {
      throw error;
    }
  }

  async SignInWithRefreshToken(): Promise<void> {
    try {
      this.signedInUser = await this.http
        .get<UserModel>('/api/authentication/token')
        .toPromise();
      this.isSignedIn = true;
      this.signedInSubject.next(true);
    } catch (error) {
      throw error;
    }
  }

  async OfflineUserHasPassword(): Promise<boolean> {
    try {
      const hasPasswordDto = await this.http
        .get<OfflineUserHasPasswordDto>('/api/authentication/offline/password')
        .toPromise();
      return hasPasswordDto.hasPassword;
    } catch (error) {
      console.error('An error occured during offline password request', error);
      return false;
    }
  }

  async OfflineSignIn(): Promise<void> {
    try {
      this.signedInUser = await this.http
        .get<UserModel>('/api/authentication/offline')
        .toPromise();
      this.isSignedIn = true;
      this.signedInSubject.next(true);
    } catch (error) {
      throw error;
    }
  }

  async SignOut(): Promise<void> {
    this.isSignedIn = false;
    this.signedInSubject.next(false);
    try {
      await this.http.get('/api/user/sign-out').toPromise();
    } catch (error) {
      throw error;
    }
  }
}
