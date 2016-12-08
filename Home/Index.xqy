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
		 <link href="http://localhost:3579/content/css/toolkit-light.min.css" rel="stylesheet"/>
	</head>
	<body>
		<div class="container">
			<div class="row">
				<div class="col sm-4">
					<h3>Badges</h3>
						<a href="/Badges">{figaro:count-category('Badges')} badges</a>
				</div>
				<div class="col sm-4">
				<h3>Comments</h3>
					<a href="/Comments">{figaro:count-category('Comments')} comments</a>
				</div>
				<div class="col sm-4">
				<h3>PostHistory</h3>
					<a href="/PostHistory">{figaro:count-category('PostHistory')} histories</a>
				</div>
				<div class="col sm-4">
				<h3>PostLinks</h3>
					<a href="/PostLinks">{figaro:count-category('PostLinks')} post links</a>
				</div>
			</div>
			<div class="row">
				<div class="col sm-4">
				<h3>Posts</h3>
					<a href="/Posts">{figaro:count-category('Posts')} posts</a>
				</div>
				<div class="col sm-4">
				<h3>Tags</h3>
					<a href="/tag">{figaro:count-category('Tags')} tags</a>
				</div>
				<div class="col sm-4">
				<h3>Users</h3>
					<a href="/users">{figaro:count-category('Users')} users</a>
				</div>
				<div class="col sm-4">
				<h3>Votes</h3>
					<a href="/badges">{figaro:count-category('Votes')} votes</a>
				</div>
			</div>
		</div>
	</body>
</html>