import { TestBed } from '@angular/core/testing';

import { TimeConversionService } from './time-conversion.service';

describe('TimeConversionService', () => {
  let service: TimeConversionService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(TimeConversionService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
