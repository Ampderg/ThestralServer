<?php
	require_once("../config.php");

	$auth_host = $GLOBALS['auth_host'];
	$auth_user = $GLOBALS['auth_user'];
	$auth_pass = $GLOBALS['auth_pass'];
	$auth_dbase = $GLOBALS['auth_dbase'];
    $db_update_password = $GLOBALS['db_update_password'];
	
	$db = mysqli_connect($auth_host, $auth_user, $auth_pass,$auth_dbase) or die("Error " . mysqli_error($db));
	

	$user_name = mysqli_real_escape_string($db,$_POST['name']);
	$client_id = mysqli_real_escape_string($db,$_POST['client_id']);
	$write_pw = mysqli_real_escape_string($db,$_POST['password']);
    if(md5($write_pw) == $db_update_password)
    {
        $sql = mysqli_query($db,"SELECT thes_connected_client_id, id FROM account WHERE user = '$user_name' AND thes_connected_client_id = '$client_id';");
        $rows = mysqli_num_rows($sql);
        if($rows == 1){
            $row = $sql->fetch_row();
            $row0 = $row[0];
            $row1 = $row[1];
            echo "$row0|$row1";
        }else{
            echo "null";
        }
    }
    else
    {
        echo "insufficient access";
    }
    mysqli_close($db);
	
?> 