using Godot;
using System;

public class Particle
{
	public int id = 0;

	public float dx = 0;
	public float dy = 0;

	public bool updated = false;

	public int frameCount = 0;

	public int lifeTime = 0;

	

	
	public Particle(int id) {
		this.id = id;
	}

	
}
