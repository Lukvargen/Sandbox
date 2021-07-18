using Godot;
using System;



public class Main : Node2D
{
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";

	

	Sprite sprite;

	

	int width = 384/1;
	int height = 216/1;
	
	Image img = new Image();
	ImageTexture imageTexture;



	int drawRadius = 2;
	int selected = 0;

	int frameCount = 0;

	enum Block : int {
		AIR,
		DIRT,
		SAND,
		WATER,
		ROCK,
		GRASS,
		WOOD,
		FIRE,
		FUSE

	};

	string[] names = {
		"Air",
		"Dirt",
		"Sand",
		"Water",
		"Rock",
		"Grass",
		"Wood",
		"Fire",
		"Fuse",
	};

	Color[] colors = {
		new Color(0,0,0,1), // air
		new Color("695B3A"), // dirt
		new Color("E1E289"), // sand
		new Color("63b0cd"), // water
		new Color("39393a"), // rock
		new Color("28502e"), // grass
		new Color("251918"),  // wood
		new Color("FB8B24"), // fire
		new Color("494947") // fuse
	};

	Particle[,] world;

	OpenSimplexNoise noise;

	


	public override void _Ready()
	{
		for (int i = 0; i < Enum.GetNames(typeof(Block)).Length; i++)
		{
			

			PackedScene r = (PackedScene)ResourceLoader.Load("res://SelectMaterialContainer.tscn");
			SelectMaterialContainer b = (SelectMaterialContainer)r.Instance();
			GetNode<VBoxContainer>("ScrollContainer/VBoxContainer").AddChild(b);
			b.Owner = this;
			b.Name = ""+i;
			b.Modulate = colors[i];
		}


		sprite = GetNode<Sprite>("Sprite");
		imageTexture = (ImageTexture)sprite.Texture;

		world = new Particle[width, height];
		noise = new OpenSimplexNoise();
		noise.Seed = (int)GD.Randi();
		noise.Octaves = 4;
		noise.Period = 128.0f;
		noise.Persistence = 0.8f;

		GD.Print("test");

		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
				world[x,y] = new Particle((int)Block.AIR);
			}
		}

		img.Create(width, height, false, Image.Format.Rgba8);
		img.Fill(new Color(0,0,0,1));
		imageTexture.CreateFromImage(img, 3);

		GenerateWorld();

	}

	private void GenerateWorld() {
		img.Lock();
		int sh = height/2;
		for (int x = 0; x < width; x++)
		{
			int noiseValue = (int)(noise.GetNoise1d(x) * 100);
			int y = sh + noiseValue;

			for (int xx = -1; xx < 2; xx++)
			{
				for (int yy = -1; yy < 2; yy++)
				{
					if (IsInside(x+xx,y+yy))
						UpdatePixel(x+xx,y+yy, new Particle((int)Block.GRASS));
				}
			}

			while (y < height-1) {
				y += 1;
				UpdatePixel(x, y, new Particle((int)Block.DIRT));
			}
		}
		img.Unlock();
		imageTexture.CreateFromImage(img);
	}


	float tickRate = 0.0f;
	float tickTime = 0.0f;

	public override void _Process(float delta)
	{
		base._Process(delta);

		if (Input.IsActionPressed("+"))
			SetDrawRadius(drawRadius+1);
		else if (Input.IsActionPressed("-"))
			SetDrawRadius(drawRadius-1);
			
		if (Input.IsActionJustPressed("air"))
			SetSelected((int)Block.AIR);
		else if (Input.IsActionJustPressed("dirt"))
			SetSelected((int)Block.DIRT);
		else if (Input.IsActionJustPressed("sand"))
			SetSelected((int)Block.SAND);
		else if (Input.IsActionJustPressed("water"))
			SetSelected((int)Block.WATER);
		else if (Input.IsActionJustPressed("rock"))
			SetSelected((int)Block.ROCK);
		else if (Input.IsActionJustPressed("grass"))
			SetSelected((int)Block.GRASS);
		else if (Input.IsActionJustPressed("wood"))
			SetSelected((int)Block.WOOD);
		else if (Input.IsActionJustPressed("fuse"))
			SetSelected((int)Block.FUSE);
		else if (Input.IsActionJustPressed("fire"))
			SetSelected((int)Block.FIRE);
		
		

		tickTime += delta;
		if (tickTime >= tickRate) {
			Tick();
			tickTime = 0;
		}

	}

	void Tick() {
		
		frameCount += 1;

		bool mouseHold = Input.IsActionPressed("click");
		Vector2 mousePos = (GetGlobalMousePosition()/sprite.Scale.x).Floor();
		img.Lock();

		if (Input.IsActionJustPressed("space")) {
			
			//img.Fill(Colors.Black);
			//imageTexture.CreateFromImage(img);
			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					UpdatePixel(x,y,new Particle(selected));
				}
			}
		}


		if (mouseHold) {
			for (int xx = -drawRadius; xx < drawRadius+1; xx++)
			{
				for (int yy = -drawRadius; yy < drawRadius+1; yy++)
				{
					if (xx*xx+yy*yy <= drawRadius*drawRadius) {
						if (IsInside((int)mousePos.x+xx, (int)mousePos.y+yy))
							UpdatePixel((int)mousePos.x+xx, (int)mousePos.y+yy, new Particle(selected));
					}
					
				}
			}
		}


		for (int y = height-1; y >= 0; y--)
		{
			if (GD.Randf() > 0.5) {
				for (int x = 0; x < width; x++)
				{
					UpdateWorld(x, y);
				}
			} else {
				for (int x = width-1; x >= 0; x--)
				{
					UpdateWorld(x, y);
				}
			}
		}
		img.Unlock();
		
		imageTexture.CreateFromImage(img, 3);
	}

	void UpdateWorld(int x, int y) {
		Particle p = world[x,y];
		p.lifeTime ++;
		if (p.frameCount == frameCount)
			return;

		int id = GetPixel(x,y);
		switch (id)
		{
			case (int)Block.AIR:
				break;
			case (int)Block.DIRT:
				UpdateDirt(x, y);
				break;
			case (int)Block.SAND:
				UpdateSand(x, y);
				break;
			case (int)Block.WATER:
				UpdateWater(x,y);
				break;
			case (int)Block.ROCK:
				break;
			case (int)Block.GRASS:
				UpdateGrass(x, y);
				break;
			case (int)Block.WOOD:
				break;
			case (int)Block.FIRE:
				UpdateFire(x, y);
				break;
			case (int)Block.FUSE:
				break;
			
		}
		p.frameCount = frameCount;
	}


	bool Fall(int x, int y, Particle p) {
		p.dy += 0.25f;
		int steps = (int)Mathf.Ceil(p.dy);
		for (int i = 0; i < steps; i++)
		{
			if (IsInside(x, y+i+1) && GetPixel(x,y+i+1) == (int)Block.AIR) {
				if (!IsInside(x, y+i+2) || GetPixel(x,y+i+2) != (int)Block.AIR || i == steps-1) {
					UpdatePixel(x,y, new Particle((int)Block.AIR));
					UpdatePixel(x,y+1+i, p);
					return true;
				}
			} 
		}
		p.dy = 0;
		return false;
	}

	bool Sink(int x, int y, Particle p) {
		if (IsInside(x, y+1) && GetPixel(x,y+1) == (int)Block.WATER) {
			UpdatePixel(x,y, world[x,y+1]);
			UpdatePixel(x,y+1, p);
			return true;
		}
		return false;
	}


	bool Slide(int x, int y, Particle p, int dir) {

		if (IsInside(x+dir,y+1) && GetPixel(x+dir, y+1) == (int)Block.AIR) {
			if (IsInside(x+dir,y) && GetPixel(x+dir, y) == (int)Block.AIR) {
				UpdatePixel(x,y, new Particle((int)Block.AIR));
				UpdatePixel(x+dir,y+1, p);
				return true;
			}
		}
		return false;
	}


	bool Flow(int x, int y, Particle p) {
		if (p.dx == 0)
			p.dx = ((float)GD.RandRange(-0.1, 0.1));
		int dir = Mathf.Sign(p.dx);
		p.dx += 0.1f * dir;
		int steps = Mathf.CeilToInt(Math.Abs(p.dx));
		if (steps > 5)
			steps = 5;

		for (int i = 1; i < steps+1; i++)
		{
			if (IsInside(x+i*dir,y) && GetPixel(x+i*dir,y) == (int)Block.AIR) {
				if (!IsInside(x+(i+1)*dir,y) || GetPixel(x+(i+1)*dir,y) != (int)Block.AIR || i == steps) {
					UpdatePixel(x,y, new Particle((int)Block.AIR));
					UpdatePixel(x+i*dir,y, p);
					return true;
				}
			}
		}
		p.dx = 0;
		return false;
	}


	bool Spread(int x, int y, Particle p) {
		if (IsInside(x, y)) {
			if (GetPixel(x,y) == (int)Block.WOOD && GD.Randf() > 0.92f) {
				Particle newP = new Particle((int)Block.FIRE);
				newP.lifeTime = -30;
				newP.frameCount = frameCount;
				UpdatePixel(x,y, newP);
				return true;

			} else if (GetPixel(x,y) == (int)Block.FUSE && GD.Randf() > 0.5f) {
				Particle newP = new Particle((int)Block.FIRE);
				newP.frameCount = frameCount;
				UpdatePixel(x,y, newP);
				return true;
			}
		}
		return false;
	}
	

	void UpdateDirt(int x, int y) {
		Particle p = world[x,y];
		if (Fall(x,y, p))
			return;
		if (Sink(x,y, p))
			return;
		if (IsInside(x, y-1) && GetPixel(x,y-1) == (int)Block.AIR) {
			if (GD.Randf() > 0.9) {
				UpdatePixel(x,y, new Particle((int)Block.GRASS));
			}
		}
	}

	void UpdateSand(int x, int y) {
		Particle p = world[x,y];
		if (Fall(x,y, p))
			return;
		if (Sink(x,y, p))
			return;
		if (Slide(x,y, p, 1))
			return;
		if (Slide(x,y, p, -1))
			return;
		
		
	}

	void UpdateWater(int x, int y) {
		Particle p = world[x,y];
		int randDir = (int)Mathf.Floor((float)GD.RandRange(-1, 2));
		if (Fall(x,y,p))
			return;
		if (Slide(x,y, p, randDir))
			return;
		if (Slide(x,y, p, -randDir))
			return;
		if (Flow(x,y,p))
			return;
		
	}

	void UpdateGrass(int x, int y) {
		Particle p = world[x,y];
		if (Fall(x, y, p))
			return;
		if (Sink(x,y, p))
			return;
		if (IsInside(x, y-1) && GetPixel(x,y-1) != (int)Block.AIR) {
			if (GD.Randf() > 0.9) {
				UpdatePixel(x,y, new Particle((int)Block.DIRT));
			}
		}
	}

	void UpdateFire(int x, int y) {
		Particle p = world[x,y];
		img.SetPixel(x,y, colors[p.id].Lightened((float)GD.RandRange(0, 0.4)));
		if (p.lifeTime > 10) {
			UpdatePixel(x,y, new Particle((int)Block.AIR));
			return;
		}

		Spread(x+1, y, p);
		Spread(x-1, y, p);
		Spread(x, y+1, p);
		Spread(x, y-1, p);
	}


	private void UpdatePixel(int x, int y, Particle p) {
		world[x,y] = p;
		img.SetPixel(x,y, colors[p.id]);
	}

	private void SwitchPixel(int x1, int y1, int x2, int y2) {
		Particle particle1 = world[x1,y1];
		Particle particle2 = world[x2,y2];

		world[x1,y1] = particle2;
		world[x2,y2] = particle1;

		img.SetPixel(x1,y1, colors[particle2.id]);
		img.SetPixel(x2,y2, colors[particle1.id]);
		
	}

	private int GetPixel(int x, int y) {
		return world[x,y].id;
	}

	private bool IsInside(int x, int y) {
		return x >= 0 && x < width && y >= 0 && y < height;
	}


	public void SetSelected(int value) {
		selected = value;
		GD.Print(value);
		GetNode<Label>("SelectedLbl").Text = "Selected: " + names[value];
	}

	void SetDrawRadius(int value) {
		drawRadius = Mathf.Clamp(value, 0, 100);
		GetNode<Label>("RadiusLbl").Text = "Radius: " + drawRadius;
	}

}
