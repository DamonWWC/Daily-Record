var str ="name=damon&age=1000";
var querystring=require("querystring");

var obj=querystring.parse(str);
console.log(obj);

console.log(querystring.stringify(obj));

//特殊字符的转义
//querystring.escape()，unescape()