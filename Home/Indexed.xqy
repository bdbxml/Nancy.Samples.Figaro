(: 
	Show items in the container sorted by name
:)

import module namespace figaro="http://modules.bdbxml.net/nancy/" at "nancy.xqm";
<html>
	<head>
		<meta charset="utf-8"/>
		<meta http-equiv="X-UA-Compatible" content="IE=edge"/>
		<meta name="viewport" content="width=device-width, initial-scale=1"/>
		<meta name="description" content="StackExchange Beer Forum on Figaro database engine"/>
		<meta name="keywords" content="Figaro, XQuery, StackExchange, beer"/>
		<meta name="author" content="Endpoint Systems"/>
		<title>Figaro Beer Microservice</title>
		 <link href="http://fonts.googleapis.com/css?family=Roboto:300,400,500,700,400italic" rel="stylesheet"/>
		 <link href="/content/css/toolkit-light.min.css" type="text/css" rel="stylesheet"/>
	</head>
	<body>
	<h1>StackExchange Beer Microservice/UI</h1>
	<p>The purpose of this page is to demonstrate mixing C# with XQuery to generate a simple (but extensible) UI.</p>
		<div class="container">		
			<div class="row">
				<div class="col-sm-5 col-md-6">
				<h3>Badges</h3>
					<a href="/badges">{$badges} badges</a>
				</div>
				<div class="col-sm-5 col-md-6">
				<h3>Comments</h3>
					<a href="/comments">{$comments} comments</a>
				</div>
				<div class="col-sm-5 col-md-6">
				<h3>PostHistory</h3>
					<a href="/postHistory">{$postHistory} histories</a>
				</div>
				<div class="col-sm-5 col-md-6">
				<h3>PostLinks</h3>
					<a href="/postlink">{$postLinks} post links</a>
				</div>
			</div>
			<div class="row">
				<div class="col-sm-5 col-md-6">
				<h3>Posts</h3>
					<a href="/post">{$posts} posts</a>
				</div>
				<div class="col-sm-5 col-md-6">
				<h3>Tags</h3>
					<a href="/tag">{$tags} tags</a>
				</div>
				<div class="col-sm-5 col-md-6">
				<h3>Users</h3>
					<a href="/users">{$users} users</a>
				</div>
				<div class="col-sm-5 col-md-6">
				<h3>Votes</h3>
					<a href="/votes">{$votes} votes</a>
				</div>
			</div>
		</div>
	</body>
</html>