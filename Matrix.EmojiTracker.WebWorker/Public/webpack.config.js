var path = require('path');
var HtmlWebpackPlugin = require("html-webpack-plugin");
var MiniCssExtractPlugin = require("mini-css-extract-plugin");
var CleanWebpackPlugin = require('clean-webpack-plugin');

module.exports = {
    entry: "./index.js",
    context: path.resolve(__dirname, 'src'),
    resolve: {
        extensions: ['.js']
    },
    module: {
        rules: [
            {
                test: /\.css$/,
                use: [
                    'style-loader',
                    MiniCssExtractPlugin.loader,
                    'css-loader',
                ]
            },
        ],
    },
    plugins: [
        new CleanWebpackPlugin({ cleanOnceBeforeBuildPatterns: ['**/*', '!.gitkeep']}),
        new MiniCssExtractPlugin({
            filename: '[name].[hash].css',
            chunkFilename: '[id].css',
        }),
        new HtmlWebpackPlugin({
            hash: true,
            inject: false,
            template: './index.html',
            filename: 'index.html' 
        }),
    ],
    output: {
        filename: '[name].[hash].js',
        path: path.resolve(__dirname, '..', 'wwwroot'),
    }
};
