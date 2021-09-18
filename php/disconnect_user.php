<?php
	require_once("../config.php");

	$auth_host = $GLOBALS['auth_host'];
	$auth_user = $GLOBALS['auth_user'];
	$auth_pass = $GLOBALS['auth_pass'];
	$auth_dbase = $GLOBALS['auth_dbase'];
    $db_update_password = $GLOBALS['db_update_password'];
	
	$db = mysqli_connect($auth_host, $auth_user, $auth_pass,$auth_dbase) or die("Error " . mysqli_error($db));
	

	$client_id = mysqli_real_escape_string($db,$_POST['client_id']);
	$write_pw = mysqli_real_escape_string($db,$_POST['password']);

    if(md5($write_pw) == $db_update_password)
    {
        mysqli_query($db,"UPDATE account SET thes_connected_client_id = NULL WHERE thes_connected_client_id = '$client_id'");
    }
    else
    {
        echo "insufficient access";
    }
	mysqli_close($db);
	
?> 