import { AfterViewInit, Component, Input } from '@angular/core';
import { SharedModule } from '../../../../shared/shared.module';
import { BaseService } from '../../../../shared/services/Base/base.service';
import * as $ from 'jquery';  // Import jQuery for use in the component
// Declare the revolution method on jQuery
declare global {
  interface JQuery {
    revolution(options: any): any;
  }
}

@Component({
  selector: 'app-slider',
  standalone: true,
  imports: [SharedModule],
  templateUrl: './slider.component.html',
  styleUrl: './slider.component.scss'
})
export class SliderComponent  implements AfterViewInit{
  constructor( public baseService: BaseService) {}
  ngAfterViewInit(): void {
    const tpj = jQuery; // Alias for jQuery

    if (tpj("#rev_slider_one").revolution === undefined) {
      this.revslider_showDoubleJqueryError("#rev_slider_one");
    } else {
      tpj("#rev_slider_one").show().revolution({
        sliderType: "standard",
        jsFileLocation: "plugins/revolution/js/",
        sliderLayout: "auto",
        dottedOverlay: "on",
        delay: 10000,
        navigation: {
          keyboardNavigation: "off",
          keyboard_direction: "horizontal",
          mouseScrollNavigation: "off",
          mouseScrollReverse: "default",
          onHoverStop: "off",
          touch: {
            touchenabled: "on",
            touchOnDesktop: "off",
            swipe_threshold: 75,
            swipe_min_touches: 1,
            swipe_direction: "horizontal",
            drag_block_vertical: false
          },
          arrows: {
            style: "metris",
            enable: true,
            hide_onmobile: true,
            hide_under: 600,
            hide_onleave: true,
            tmp: '',
            left: {
              h_align: "left",
              v_align: "center",
              h_offset: 15,
              v_offset: 0
            },
            right: {
              h_align: "right",
              v_align: "center",
              h_offset: 15,
              v_offset: 0
            }
          }
        },
        responsiveLevels: [1200, 1040, 802, 480],
        visibilityLevels: [1200, 1040, 802, 480],
        gridwidth: [1200, 1040, 800, 480],
        gridheight: [700, 700, 700, 700],
        lazyType: "none",
        parallax: {
          type: "mouse",
          origo: "enterpoint",
          speed: 1000,
          levels: [1, 2, 3, 4, 5]
        },
        shadow: 0,
        spinner: "off",
        stopLoop: "off",
        stopAfterLoops: -1,
        stopAtSlide: -1,
        shuffle: "off",
        autoHeight: "off",
        hideThumbsOnMobile: "off",
        hideSliderAtLimit: 0,
        hideCaptionAtLimit: 0,
        hideAllCaptionAtLilmit: 0,
        debugMode: false,
        fallbacks: {
          simplifyAll: "off",
          nextSlideOnWindowFocus: "off",
          disableFocusListener: false
        }
      });
    }
  }

  private revslider_showDoubleJqueryError(selector: string): void {
    console.error(`Revolution Slider Error: Double jQuery issue with ${selector}`);
  }
}
