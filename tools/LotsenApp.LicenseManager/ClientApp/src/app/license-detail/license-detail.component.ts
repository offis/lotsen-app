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

import {Component, ElementRef, Input, OnDestroy, OnInit, ViewChild} from '@angular/core';
import {ILicenseInformation} from "../ilicense-information";
import {LicenseService} from "../license.service";
import {Subscription} from "rxjs";
import {MatInput} from "@angular/material/input";
import {FormControl} from "@angular/forms";

@Component({
  selector: 'app-license-detail',
  templateUrl: './license-detail.component.html',
  styleUrls: ['./license-detail.component.scss']
})
export class LicenseDetailComponent implements OnInit, OnDestroy {

  private licenseDetailSubscription!: Subscription;

  @Input()
  licenses: ILicenseInformation[] = [];

  licenseTextFormControl = new FormControl('');
  licenseTextHtmlFormControl = new FormControl('');

  selectedLicense?: ILicenseInformation;

  constructor(private licenseService: LicenseService) { }

  ngOnInit(): void {
    this.licenseDetailSubscription = this.licenseService.LicenseDetail.subscribe(i => this.showLicense(i));
  }

  ngOnDestroy(): void {
    this.licenseDetailSubscription.unsubscribe();
  }

  showLicense(identifier: string): void {
    this.selectedLicense = this.licenses.find(l => l.licenseId === identifier);
    this.licenseTextFormControl.setValue(this.selectedLicense?.licenseText);
    this.licenseTextHtmlFormControl.setValue(this.selectedLicense?.licenseTextHtml);
  }

  async updateLicense(): Promise<void> {
    if(!this.selectedLicense) {
      return;
    }
    this.selectedLicense.licenseText = this.licenseTextFormControl.value;
    this.selectedLicense.licenseTextHtml = this.licenseTextHtmlFormControl.value;
    await this.licenseService.UpdateLicense(this.selectedLicense);
  }

}
