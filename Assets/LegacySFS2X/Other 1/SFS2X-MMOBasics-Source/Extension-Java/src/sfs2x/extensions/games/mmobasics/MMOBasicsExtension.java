package sfs2x.extensions.games.mmobasics;

import java.util.Arrays;
import java.util.HashMap;
import java.util.LinkedList;
import java.util.List;
import java.util.Map;
import java.util.Random;
import java.util.concurrent.ScheduledFuture;
import java.util.concurrent.TimeUnit;

import com.smartfoxserver.v2.SmartFoxServer;
import com.smartfoxserver.v2.api.ISFSMMOApi;
import com.smartfoxserver.v2.core.ISFSEvent;
import com.smartfoxserver.v2.core.SFSEventParam;
import com.smartfoxserver.v2.core.SFSEventType;
import com.smartfoxserver.v2.entities.User;
import com.smartfoxserver.v2.entities.variables.SFSUserVariable;
import com.smartfoxserver.v2.entities.variables.UserVariable;
import com.smartfoxserver.v2.exceptions.SFSException;
import com.smartfoxserver.v2.extensions.BaseServerEventHandler;
import com.smartfoxserver.v2.extensions.SFSExtension;
import com.smartfoxserver.v2.mmo.MMORoom;
import com.smartfoxserver.v2.mmo.Vec3D;

public class MMOBasicsExtension extends SFSExtension
{
	private static final int NUM_NPC = 50;
	private static final int NUM_MODELS = 3;
	private static final int NUM_MATERIALS = 4;

	private MMORoom mmoRoom;
	private NPCRunner npcRunner;
	private ISFSMMOApi mmoAPi;
	
	private boolean simulationStarted = false;
	private ScheduledFuture<?> npcRunnerTask;
	private List<User> allNpcs;
	
	@Override
	public void init()
	{
		mmoRoom = (MMORoom) this.getParentRoom();
	    npcRunner = new NPCRunner();
	    mmoAPi = SmartFoxServer.getInstance().getAPIManager().getMMOApi();
	    
	    // Add event listener
	    addEventHandler(SFSEventType.USER_VARIABLES_UPDATE, new UserVariablesUpdateHandler());
	}
	
	@Override
	public void destroy()
	{
	    super.destroy();
	    
	    // Destroy NPCRunnerTask
	    if (npcRunnerTask != null)
	    	npcRunnerTask.cancel(true);
	}
	
	/**
	 * Create Non-Player Characters and start task making them move around the map. 
	 */
	private void simulatePlayers() throws Exception
	{
		allNpcs = new LinkedList<User>();
		Random rnd = new Random();
		
		// Create NPCs
		for (int i = 0; i < NUM_NPC; i++)
			allNpcs.add(getApi().createNPC("NPC#" +i, getParentZone(), false));
		
		for (User user : allNpcs)
        {
			// Set random position within min & max map limits
			// Y coordinate is ignored as map is assumed to be flat; Y will be evaluated on the client side based on terrain elevation
			int rndX = rnd.nextInt(mmoRoom.getMapHigherLimit().intX() - mmoRoom.getMapLowerLimit().intX()) + mmoRoom.getMapLowerLimit().intX();
			int rndZ = rnd.nextInt(mmoRoom.getMapHigherLimit().intZ() - mmoRoom.getMapLowerLimit().intZ()) + mmoRoom.getMapLowerLimit().intZ();
			
			Vec3D rndPos = new Vec3D((float) rndX, 0, (float) rndZ);
			
			List<UserVariable> uVars = Arrays.asList
			(
				(UserVariable) new SFSUserVariable("x", (double) rndPos.floatX()),
				new SFSUserVariable("y", (double) rndPos.floatY()),
				new SFSUserVariable("z", (double) rndPos.floatZ()),
				new SFSUserVariable("rot", (double) rnd.nextInt(360)),
				new SFSUserVariable("model", rnd.nextInt(NUM_MODELS - 1)), 
				new SFSUserVariable("mat", rnd.nextInt(NUM_MATERIALS - 1))
			);
			
			// Set random speed and save it in user properties, which are server-side data only
			NPCData data = new NPCData();
			data.xspeed = rnd.nextFloat() * 1.2f;
			data.zspeed = rnd.nextFloat() * 1.2f;
			
			user.setProperty("npcData", data);
					
			// Set NPC's User Variables
			getApi().setUserVariables(user, uVars);
			
			// Make NPC join the MMORoom
			getApi().joinRoom(user, mmoRoom);
			
			// Set NPC position in proximity system
			mmoAPi.setUserPosition(user, rndPos , mmoRoom);
        }
		
		// Start task making NPCs move around
		npcRunnerTask = SmartFoxServer.getInstance().getTaskScheduler().scheduleAtFixedRate(
			npcRunner, 			
			0, 							// 0 initial delay
			100, 						// run every 100ms
			TimeUnit.MILLISECONDS		
		);
	}
	
	
	//===================================================================================================
	
	
	/**
	 * Class handling the User Variable Update event occurring in the parent Room.
	 * 
	 * If the User Variables representing user position along x or z axis are updated,
	 * update the position in MMORoom's proximity manager too to make users appear in each other's Area of Interest.
	 * 
	 * Position along the vertical axis is not considered because game map is assumed to be flat,
	 * and height is evaluated on the client side only based on terrain elevation.
	 */
	private class UserVariablesUpdateHandler extends BaseServerEventHandler
	{
		@Override
		public void handleServerEvent(ISFSEvent event) throws SFSException
		{
			// If not yet done, start the simulation which creates and controls Non-Player Characters (NPC)
			// NOTE: we can't do it during the Extension initialization because MMORoom-specific settings are available yet
			if (!simulationStarted)
			{
				try
		        {
					simulationStarted = true;
					
			    	// Create NPCs running around the map
			        simulatePlayers();
		        }
		        catch (Exception e)
		        {
		        	e.printStackTrace();
		        }
			}
			
			//------------------------------------------------------------------------------------------------------
			
			@SuppressWarnings("unchecked")
            List<UserVariable> variables = (List<UserVariable>) event.getParameter(SFSEventParam.VARIABLES);
			User user = (User) event.getParameter(SFSEventParam.USER);
			
			// Make a map of the variables list
			Map<String, UserVariable> varMap = new HashMap<String, UserVariable>();
			
			for (UserVariable var : variables)
				varMap.put(var.getName(), var);
			
			if (varMap.containsKey("x") || varMap.containsKey("z"))
			{
				// Extract position from User Variables
				Vec3D pos = new Vec3D(
					varMap.get("x").getDoubleValue().floatValue(),
					0f,
					varMap.get("z").getDoubleValue().floatValue()
				);
				
				// Set position in proximity system
				mmoAPi.setUserPosition(user, pos, getParentRoom());
			}
		}
	}
	
	/**
	 * Runnable class responsible of moving the Non-Player Characters on the map, updating their x and z coordinates.
	 * 
	 * Y coordinate is evaluated on the client side only based on terrain elevation. 
	 */
	private class NPCRunner implements Runnable
	{
		@Override
		public void run()
		{
			for (User npc : allNpcs)
			{
				NPCData data = (NPCData) npc.getProperty("npcData");
				
				double xpos = npc.getVariable("x").getDoubleValue();
				double zpos = npc.getVariable("z").getDoubleValue();
				
				double newX = xpos + data.xspeed;
				double newZ = zpos + data.zspeed;
				
				// Check map x limits
				if (newX < mmoRoom.getMapLowerLimit().floatX() || newX > mmoRoom.getMapHigherLimit().floatX())
				{
					newX = xpos;
					data.xspeed *= -1;
				}
				
				// Check map z limits
				if (newZ < mmoRoom.getMapLowerLimit().floatZ() || newZ > mmoRoom.getMapHigherLimit().floatZ())
				{
					newZ = zpos;
					data.zspeed *= -1;
				}
				
				// Set NPC variables
				List<UserVariable> vars = Arrays.asList
				(
					(UserVariable)
					new SFSUserVariable("x", newX), 
					new SFSUserVariable("z", newZ)
				);
				
				// Set NPC position in its User Variables
				getApi().setUserVariables(npc, vars);
			}
		}
	}
	
	/**
	 * Utility class.
	 */
	private final static class NPCData
	{
		public float xspeed;
		public float zspeed;
	}
}
