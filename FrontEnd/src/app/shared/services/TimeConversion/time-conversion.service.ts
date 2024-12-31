import { Injectable } from '@angular/core';
import * as moment from 'moment-timezone';


@Injectable({
  providedIn: 'root',
})
export class TimeConversionService {
  constructor() {}

  convertLocalTimeToUtc(localTime: any): string {
    const timeZone = moment.tz.guess();
    const localDate = moment.tz(localTime, timeZone, 'HH:mm:ss');
    return localDate.utc().format('HH:mm:ss');
  }

  convertUtcTimeToLocal(utcTime: any): string {
    const timeZone = moment.tz.guess();
    const utcDate = moment.utc(utcTime, 'HH:mm:ss');
    return utcDate.tz(timeZone).format('hh:mm:ss A');
  }

  convertLocalDateTimeToUtc(localDateTime: any): string {
    const timeZone = moment.tz.guess();
    const localDate = moment.tz(localDateTime, timeZone);
    return localDate.utc().format('YYYY-MM-DDTHH:mm:ss');
  }

  convertUtcDateTimeToLocal(utcDateTime: any): string {
    const timeZone = moment.tz.guess();
    const utcDate = moment.utc(utcDateTime, 'YYYY-MM-DDTHH:mm:ss');
    return utcDate.tz(timeZone).format('YYYY-MM-DDTHH:mm:ss');
  }
}
