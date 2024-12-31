import { BaseService } from './../../services/Base/base.service';
import { Component, Input, OnInit } from '@angular/core';
import { RouterModule } from '@angular/router';
import { SharedModule } from '../../shared.module';

@Component({
  selector: 'app-NoData',
  standalone: true,
  imports: [RouterModule, SharedModule],
  templateUrl: './NoData.component.html',
  styleUrls: ['./NoData.component.css'],
})
export class NoDataComponent implements OnInit {
  @Input() message: string = 'No results yet';
  @Input() description: string =
    'Please select the genetic test that suits you so that its results appear to you.';
  @Input() button1Label: string = 'Home';
  @Input() button2Label: string = 'Genetic Tests';
  @Input() button1Link: string = '/';
  @Input() button2Link: string = '/genetic-tests';
  @Input() showButton: boolean = true;
  constructor(public BaseService: BaseService) {}

  ngOnInit() {}
}
