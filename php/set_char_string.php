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
    $char_str = mysqli_real_escape_string($db,$_POST['character_string']);
	$write_pw = mysqli_real_escape_string($db,$_POST['password']);

    if(md5($write_pw) == $db_update_password)
    {

        $sql = "SELECT * FROM THES_Characters
        INNER JOIN account ON account.id = THES_Characters.user_id
        WHERE THES_Characters.user_id = $user_id AND THES_Characters.user_character_id = $char_id;";

        $response = mysqli_query($db, $sql);
        if(mysqli_num_rows($response) == 0)
        {
            $sql = "INSERT INTO THES_Characters (user_id, character_name, character_appearance, user_character_id)
            VALUES($user_id, 'Pony', '$char_str', $char_id);";

            $response = mysqli_query($db, $sql);

            echo "character created: $response";
        }
        else
        {
            $sql = "UPDATE THES_Characters c, account a
            SET c.character_appearance = '$char_str'
            WHERE a.id = $user_id AND c.user_character_id = $char_id
            AND c.user_id = a.id;";
                    
            $response = mysqli_query($db, $sql);

            echo "character updated: $response";
        }

        //TODO: if this changes more than one line, rollback
    }
    else
    {
        echo "insufficient access";
    }
	mysqli_close($db);
	
?> 