(: 
	Show a paged list of users
:)

(: import module namespace figaro="http://modules.bdbxml.net/nancy/" at "nancy.xqm"; :)

(: declare function nancy:decode($s as xs:string) as xs:string external;
declare function xqilla:parse-html($html as xs:string?) as document-node()? external; :)

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
		<h1>StackExchange Beer Users</h1>
		<p>There are {$users} users.</p>
		<table class="table table-hover table-condensed">
			<thead>
				<tr>
					<td>Id</td>
					<td>Reputation</td>
					<td>CreationDate</td>
					<td>DisplayName</td>
					<td>LastAccessDate</td>
					<td>WebsiteUrl</td>
					<td>Location</td>
					<td>About Me</td>
					<td>Views</td>
					<td>UpVotes</td>
					<td>DownVotes</td>
					<td>AccountId</td>
				</tr>
			</thead>
			<tbody>
				{
					let $rows := for $x in collection('beer') where dbxml:metadata('db:category',$x) = $cat return $x
					
					for $row at $count in $rows where $count = ($a to $b) 						
						return 
						<tr>
							<td>{xs:string($row/row/@Id)}</td>
							<td>{xs:string($row/row/@Reputation)}</td>
							<td>{xs:string($row/row/@CreationDate)}</td>
							<td>{xs:string($row/row/@DisplayName)}</td>
							<td>{xs:string($row/row/@LastAccessDate)}</td>
							<td><a href="{xs:string($row/row/@WebsiteUrl)}">{xs:string($row/row/@WebsiteUrl)}</a></td>
							<td>{xs:string($row/row/@Location)}</td>
							<td>{xqilla:parse-html(xs:string($row/row/@AboutMe))}</td>
							<td>{xs:string($row/row/@Views)}</td>
							<td>{xs:string($row/row/@UpVotes)}</td>
							<td>{xs:string($row/row/@DownVotes)}</td>
							<td>{xs:string($row/row/@AccountId)}</td>
						</tr>
				}
			</tbody>
		</table>
	</body>
</html>