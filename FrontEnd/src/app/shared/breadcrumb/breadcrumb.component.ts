import { Component, Input } from '@angular/core';
import { SharedModule } from '../shared.module';
import { BaseService } from '../services/Base/base.service';
export interface Breadcrumb {
  label: string;
  url: string;
  last?: boolean; 
}
@Component({
  selector: 'app-breadcrumb',
  standalone: true,
  imports: [SharedModule],
  templateUrl: './breadcrumb.component.html',
  styleUrl: './breadcrumb.component.scss',
})
export class BreadcrumbComponent {
  @Input() breadcrumbs: Breadcrumb[] = [];
  constructor(public baseSerive: BaseService) {}
}
