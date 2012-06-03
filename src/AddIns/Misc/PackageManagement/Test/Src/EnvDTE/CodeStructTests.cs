﻿// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;
using ICSharpCode.PackageManagement.EnvDTE;
using ICSharpCode.SharpDevelop.Dom;
using NUnit.Framework;
using PackageManagement.Tests.Helpers;

namespace PackageManagement.Tests.EnvDTE
{
	[TestFixture]
	public class CodeStructTests
	{
		CodeStruct codeStruct;
		ProjectContentHelper helper;
		IClass fakeStruct;
		
		[SetUp]
		public void Init()
		{
			helper = new ProjectContentHelper();
		}
		
		void CreatePublicStruct(string name)
		{
			fakeStruct = helper.AddPublicStructToProjectContent(name);
			CreateStruct();
		}
		
		void CreatePrivateStruct(string name)
		{
			fakeStruct = helper.AddPrivateStructToProjectContent(name);
			CreateStruct();
		}
		
		void CreateStruct()
		{
			codeStruct = new CodeStruct(helper.FakeProjectContent, fakeStruct);
		}

		[Test]
		public void Access_PublicStruct_ReturnsPublic()
		{
			CreatePublicStruct("MyStruct");
			
			vsCMAccess access = codeStruct.Access;
			
			Assert.AreEqual(vsCMAccess.vsCMAccessPublic, access);
		}
		
		[Test]
		public void Access_PrivateStruct_ReturnsPrivate()
		{
			CreatePrivateStruct("MyStruct");
			
			vsCMAccess access = codeStruct.Access;
			
			Assert.AreEqual(vsCMAccess.vsCMAccessPrivate, access);
		}
		
		[Test]
		public void Kind_PublicStruct_ReturnsStruct()
		{
			CreatePublicStruct("MyStruct");
			
			vsCMElement kind = codeStruct.Kind;
			
			Assert.AreEqual(vsCMElement.vsCMElementStruct, kind);
		}
	}
}
