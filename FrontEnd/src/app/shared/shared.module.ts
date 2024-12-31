import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { register } from 'swiper/element/bundle';

const modules = [
  TranslateModule,
  CommonModule,
  FormsModule,
  ReactiveFormsModule,
  RouterModule,
  
];
register();
@NgModule({
  imports: modules,
  exports: [ ...modules],
})
export class SharedModule {}
