@extends('defaults::layouts.master')

@section('content')
    <div class="banner-header container-fluid" style="padding:0 !important;">
        <div class="swiper-container">
            <div class="swiper-wrapper">
                <div class="swiper-slide">
                    <img src="images/banner2.jpg">
                    <div class="container">
                        <div class="col-md-6 text-banner">
                            <div class="center-banner">
                                <h2>HPSTANDARD.COM</h2>
                                <h1>AT YOUR SERVICE</h1>
                                <h4>WE HELP ORGANIZATIONS IMPROVE THEIR PERFORMANCE</h4>
                                <div class="bt-read">Read more <i class="fa fa-caret-right" aria-hidden="true"></i>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="swiper-slide">
                    <img src="images/banner2.jpg">
                    <div class="container">
                        <div class="col-md-6 text-banner">
                            <div class="center-banner">
                                <h2>HPSTANDARD.COM</h2>
                                <h1>AT YOUR SERVICE</h1>
                                <h4>WE HELP ORGANIZATIONS IMPROVE THEIR PERFORMANCE</h4>
                                <div class="bt-read">Read more <i class="fa fa-caret-right" aria-hidden="true"></i>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            <div class="swiper-pagination"></div>
        </div>
    </div>

    @include('defaults::home.partials.about')
    @include('defaults::home.partials.product_services')
    @include('defaults::home.partials.department')
    @include('defaults::home.partials.news_stock')
    @include('defaults::home.partials.contact')
@endsection
