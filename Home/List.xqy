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
        <div class="container">
            <table class="table table-striped table-hover table-condensed">
                <thead>
                    <tr>
                        <td>Id</td>
                        <td>UserId</td>
                        <td>Name</td>
                        <td>Date</td>
                        <td>Class</td>
                        <td>TagBased</td>
                    </tr>
                </thead>
                {
                    for $x in collection('beer') where where dbxml:metadata('dbxml:name', $x)[contains(.,$term)]
                }
            </table>
        </div>
    </body>
</html>