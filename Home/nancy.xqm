(: 
	Nancy demo XQuery module
:)

module namespace figaro = 'http://modules.bdbxml.net/nancy/';


declare function figaro:count-category(
	$cat as item()
)
{
	fn:count(for $x in collection('beer') where dbxml:metadata('db:category',$x) = $cat return $x)
};

(:~
 : Returns the search results for the different items we have stored in our Beer service database.
 : Note: 
 : This method scans each name to do a contains() function on the string value, so as a result it's very slow. We're
 : including it here as an example.
 : 
 : @param $term the term to search for
:)
declare function figaro:look-for(
	$term as item()
)
{
	fn:count(for $x in collection("beer.dbxml") where dbxml:metadata('dbxml:name', $x)[contains(.,$term)]
	return $x)
};