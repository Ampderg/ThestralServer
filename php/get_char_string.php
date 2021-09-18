<?php
	require_once("../config.php");

	$auth_host = $GLOBALS['auth_host'];
	$auth_user = $GLOBALS['auth_user'];
	$auth_pass = $GLOBALS['auth_pass'];
	$auth_dbase = $GLOBALS['auth_dbase'];
    $db_update_password = $GLOBALS['db_update_password'];
	
	$db = mysqli_connect($auth_host, $auth_user, $auth_pass,$auth_dbase) or die("Error " . mysqli_error($db));
	

	$user_id = mysqli_real_escape_string($db,$_POST['user_id']);
    $char_id = mysqli_real_escape_string($db,$_POST['char_id']);
	$write_pw = mysqli_real_escape_string($db,$_POST['password']);

    if(md5($write_pw) == $db_update_password)
    {
        $sql = "SELECT character_appearance FROM THES_Characters
        INNER JOIN account ON account.id = THES_Characters.user_id
        WHERE THES_Characters.user_id = $user_id AND THES_Characters.user_character_id = $char_id;";

        $result = mysqli_query($db, $sql);
        $rows = mysqli_num_rows($result);
        if($rows == 1){
            $s = $result->fetch_row()[0];
            echo "$s";
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