library IEEE;
use IEEE.STD_LOGIC_1164.ALL;
use ieee.numeric_std.all;

entity $name is
	Port (  
			clk 				: 	in  	STD_LOGIC;
			inputs				:	in		STD_LOGIC_VECTOR($ports downto 0)
			);
end $name;

architecture Behavioral of $name is
	shared variable curState	:	std_logic_vector($width downto 0) := "$zero";
	shared variable newState	:	std_logic_vector($width downto 0) := "$zero";
begin

process (clk) begin     
	if clk = '1' and clk'event then
$rules
		curState := newState;
	end if;
end process;

end Behavioral;

