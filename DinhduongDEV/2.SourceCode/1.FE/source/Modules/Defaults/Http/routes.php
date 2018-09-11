<?php

Route::group(['middleware' => 'web', 'prefix' => '', 'namespace' => 'Modules\Defaults\Http\Controllers'], function()
{
    Route::get('/', 'HomeController@index');
});
