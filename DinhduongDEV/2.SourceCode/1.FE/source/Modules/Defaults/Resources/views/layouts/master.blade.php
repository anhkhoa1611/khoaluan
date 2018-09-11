<!DOCTYPE html>
<html lang="en">
    <head>
        <base href="{{url('')}}">
        <meta charset="utf-8">
        <meta http-equiv="X-UA-Compatible" content="IE=edge">
        <meta name="viewport" content="width=device-width, initial-scale=1">
        @yield('title')


        <link rel="stylesheet" href="https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/css/bootstrap.min.css">
        <link rel="stylesheet" href="fonts/font-awesome-4.7.0/css/font-awesome.min.css">
        <link rel="stylesheet" type="text/css" href="css/styles.css">
        <link rel="stylesheet" type="text/css" href="css/swiper.min.css">
        @yield('styles')
        @yield('scripts-head')
        
    </head>
    <body>
            @include('defaults::layouts.header')

            @yield('content')

            @include('defaults::layouts.footer')
            
            <script src="https://ajax.googleapis.com/ajax/libs/jquery/3.2.1/jquery.min.js"></script>
            <script src="https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/js/bootstrap.min.js"></script>
            <script src="js/swiper.min.js"></script>
            @yield('scripts')
            <script type="text/javascript">
                var swiper = new Swiper('.swiper-container', {
                    pagination: '.swiper-pagination',
                    paginationClickable: true,
                    spaceBetween: 30,
                    centeredSlides: true,
                    autoplay: 2500,
                    autoplayDisableOnInteraction: false,
                    loop:true
                });
                $(document).ready(function() {

                    $('#contactlink').click = function (){
                        $(document).scrollTo('#contact');
                    }

                });
            </script>

    </body>

</html>
