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

import { Component, Input, OnInit } from '@angular/core';
import { LocalisationConfiguration } from '../../../core/localisation-configuration';
import { MatSelectChange } from '@angular/material/select';
import { TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'la2-localisation-configuration',
  templateUrl: './localisation-configuration.component.html',
  styleUrls: ['./localisation-configuration.component.scss'],
})
export class LocalisationConfigurationComponent implements OnInit {
  @Input()
  configuration!: LocalisationConfiguration;

  standardDateFormats = [
    'HH:mm:ss dd.MM.YY',
    'YYYY-MM-ddTHH:mm:ss',
    'dd-MM-YYYY HH:mm:ss',
    'dd MMM YYYY HH:mm:ss',
    'dd MMMM YYYY HH:mm:ss',
    'EEEE, dd MMM YYYY HH:mm:ss',
    'dd MMMM YYYY',
  ];

  currentTimeStamp = new Date();
  constructor(private translate: TranslateService) {}

  ngOnInit(): void {
    setInterval(() => {
      this.currentTimeStamp = new Date();
    }, 1000);
  }

  updateLanguage(event: MatSelectChange) {
    console.log(event, this.configuration);
    this.translate.use(this.configuration.language);
  }

  updateCurrency(event: MatSelectChange) {
    this.configuration.currency = event.value;
  }

  updateDate(event: MatSelectChange) {
    this.configuration.dateFormat = event.value;
  }
}
