<?php


/* enqueue script for parent theme stylesheeet */
function childtheme_parent_styles() {

    // enqueue style for parent theme
    wp_enqueue_style( 'parent', get_template_directory_uri().'/style.css' );

    // child theme css
    wp_enqueue_style( 'wedding-style', get_stylesheet_directory_uri().'/style.css' );

    // child theme js
    wp_register_script( 'wedding-script', get_stylesheet_directory_uri() . '/script.js',
        array( 'jquery'), '1.0', true);

    wp_enqueue_script('wedding-script');
}

add_action( 'wp_enqueue_scripts', 'childtheme_parent_styles');

