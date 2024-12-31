// import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from "@angular/common/http";
// import { Injectable, inject } from "@angular/core";
// import { Observable } from "rxjs";
// import { catchError } from "rxjs/operators";
// import { HendleErrorService } from "../services/HandleError/hendle-error-service";

// @Injectable()
// export class HandleErrorInterceptor implements HttpInterceptor {
//     handleErrorService = inject(HendleErrorService);

//     intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
//         return next.handle(req).pipe(
//             catchError(this.handleErrorService.logErrorResponse)
//         );
//     }
// }
