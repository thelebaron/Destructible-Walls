![](fractured_wall.gif)

#### DOTS destructibles

##### Testing Notes
At largeish amounts, the connection graph system does not scale well. A scene with 121 breakable entities at 50 parts each(6k entities total), the graph ms usage is 2ms.

An entity's sub parts should not be enabled until actually needed(Entity swap).

Need a cleanup system(thousands of physics entities is kind of undesirable even with the job system and a beefy cpu).

Connection graphs still exist when a destructible is completely "destructed".

Use chunk filtering to ignore unchanged chunks.

